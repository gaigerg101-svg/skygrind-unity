// NIGHT36 builder P (the core loop): the P rows' headless judges. ONE entry, N36Core(cmd, args, ref code) (declared and called in Program.cs).
//   n36core [--off36]   each check runs with its row ON, then OFF (the RED side, L-014), on the SURVIVAL map, invulnerable, the round clock held:
//     tells     a husk walks into melee reach (no punch): a YELLOW huskMelee tell of design.n36.tells36.huskWindupS; a spitter 12 m out: a
//               YELLOW spitterVolley tell of spitterTellS. OFF: no tell, the web's 0.5 s wind-up.
//     parry     a spitter pinned 12 m ahead lobs a volley at you (thrown directly, never the deliberate miss); the punch is pressed as the
//               first glob comes within orb.reflectRange: the glob goes back at the spitter (its target = the shooter), bursts, and the
//               spitter (60 hp) dies (24 return + 40 burst). "parry36" once; the step's hitstop >= freezeS. OFF: no burst, spitter alive.
//     buffer    the same volley, the punch pressed ~0.08 s BEFORE the first glob is in reach (the glob not yet reflectable): parried by the
//               early buffer (parry36 weapon "parry36buffer"). OFF: the early punch whiffs, no parry.
//     launch    a husk 6 m ahead: dash at it, punch when it is in reach: launch36 once, the husk thrown >= 4 m along the punch. A STANDING
//               punch with the row on: no launch (speed pays, standing still pays nothing). OFF: the punch's knockback only.
//     twokill   husk A in reach after a dash, husk B 4 m beyond A on the punch's line (both 40 hp): A flies into B: a burst, two kills.
//               OFF: no kill.
//     heal      hp 50; a 1 hp husk 2.5 m ahead shot dead: +design.n36.heal36.amount ("heal36"); a 1 hp husk 12 m ahead shot dead: no heal.
//   GREEN = every ON line passes and every OFF line shows the old rule.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N36Core(string cmd, string[] args, ref int code)
    {
        if (cmd == "n36core") N36CoreCommand(args, ref code);
    }

    static SurvivalSim Core36Sim(GameConfig cfg, N36Blocks rows, uint seed, out Vec3 sp, out double yaw, out double ux, out double uz, double open = 15)
    {
        Core36Keep.Clear();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n36Override = rows;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.holdRunClock = true;
        sim.mover.invulnerable = true;
        sp = sim.map.playerSpawn;
        sim.Respawn(sp, 0);
        (yaw, ux, uz) = Core36OpenDir(sim, sp, open);
        foreach (Enemy e in sim.enemies.all) e.alive = false;
        return sim;
    }

    /// <summary>A direction from p with `dist` m of open floor: spheres (0.5 m) every 0.5 m at 0.6 and 1.4 m up touch nothing (the core grid's
    /// cells included, which the eye rays of ArclineOpenDir miss).</summary>
    static (double yaw, double dx, double dz) Core36OpenDir(SurvivalSim sim, Vec3 p, double dist)
    {
        for (int k = 0; k < 64; k++)
        {
            double a = k * Math.PI / 32;
            double dx = Math.Sin(a), dz = -Math.Cos(a);
            bool clear = true;
            for (double s = 1; s <= dist && clear; s += 0.5)
                foreach (double h in new[] { 0.6, 1.4 })
                {
                    var c = new Vec3(p.x + dx * s, p.y + h, p.z + dz * s);
                    if (WorldCollider.CapsuleIntersect(sim.world, new Capsule { start = c, end = c, radius = 0.5 }, out _, out _)) { clear = false; break; }
                }
            if (clear) return (Math.Atan2(-dx, -dz), dx, dz);
        }
        Console.WriteLine($"    (no open floor of {dist} m from ({p.x:0},{p.z:0}))");
        return (0, 0, -1);
    }

    static Husk Core36Spawn(SurvivalSim sim, string kind, Vec3 sp, double ux, double uz, double d, double hp)
    {
        for (int k = 0; k < 6; k++)
        {
            Husk h = sim.husks.Spawn(kind, new Vec3(sp.x + ux * d, sp.y + 0.25 * k, sp.z + uz * d), sp);
            if (h == null) continue;
            if (hp > 0) h.hp = h.maxHp = hp;
            Core36Keep.Add(h);
            return h;
        }
        return null;
    }

    static void Core36Tick(SurvivalSim sim, double yaw, double pitch, CombatInput c, InputFrame f = default)
    {
        // only the check's own bodies live (a run's other enemies would catch a glob or a flier)
        foreach (Enemy e in sim.enemies.all) if (e.alive && !Core36Keep.Contains(e)) e.alive = false;
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, pitch);
        inp.Set(f, c);
        sim.hitstopLeft = 0;
        sim.Step();
    }

    static readonly HashSet<Enemy> Core36Keep = new HashSet<Enemy>();
    static readonly CombatInput NoCombat36= new CombatInput { weapon = -1 };
    static readonly CombatInput Punch36 = new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } };

    // ---- tells ----
    static string Core36Tells(GameConfig cfg, bool on, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { tells36 = on }, 3601, out Vec3 sp, out double yaw, out double ux, out double uz);
        var seen = new List<string>();
        int huskSteps = -1, spitSteps = -1;
        sim.events.On<Tell36>(Tells36.Event, t =>
        {
            seen.Add($"{t.attack} {t.kind} {t.windupSteps * dt:0.00}s");
            if (t.attack == SurvivalSim.Tell36HuskMelee && huskSteps < 0) huskSteps = t.windupSteps;
            if (t.attack == SurvivalSim.Tell36SpitterVolley && spitSteps < 0) spitSteps = t.windupSteps;
        });
        int windupWeb = -1;
        Husk h = Core36Spawn(sim, "husk", sp, ux, uz, 1.4, 1e9);
        Husk s = Core36Spawn(sim, "spitter", sp, ux, uz, 12, 1e9);
        for (int i = 0; i < MovementMath.StepsFor(4, dt); i++)
        {
            if (s != null && s.alive) { s.position = new Vec3(sp.x + ux * 12, s.position.y, sp.z + uz * 12); }
            if (h != null && h.windupSteps > 0 && windupWeb < 0) windupWeb = h.windupSteps;
            Core36Tick(sim, yaw, 0, NoCombat36);
        }
        double want = cfg.Num("design.n36.tells36.huskWindupS"), wantS = cfg.Num("design.n36.tells36.spitterTellS");
        string line = $"tells36 {(on ? "ON " : "OFF")}: tells [{string.Join(", ", seen.GetRange(0, Math.Min(4, seen.Count)))}{(seen.Count > 4 ? $", +{seen.Count - 4}" : "")}], husk wind-up at its start {(windupWeb < 0 ? "none" : (windupWeb * dt).ToString("0.00") + " s")}, yellows {sim.tells36.yellows} reds {sim.tells36.reds}";
        if (on)
        {
            if (huskSteps != MovementMath.StepsFor(want, dt)) fails.Add($"tells ON: huskMelee tell {huskSteps} steps, want {MovementMath.StepsFor(want, dt)}");
            if (spitSteps != MovementMath.StepsFor(wantS, dt)) fails.Add($"tells ON: spitterVolley tell {spitSteps} steps, want {MovementMath.StepsFor(wantS, dt)}");
        }
        else
        {
            if (seen.Count != 0) fails.Add("tells OFF: a tell was raised");
            if (windupWeb >= 0 && windupWeb != MovementMath.StepsFor(cfg.Num("husk.windupTime"), dt) - 1 && windupWeb != MovementMath.StepsFor(cfg.Num("husk.windupTime"), dt)) fails.Add($"tells OFF: husk wind-up {windupWeb} steps, not the web's");
        }
        return line;
    }

    // ---- parry (on time, or early: the buffer) ----
    static string Core36Parry(GameConfig cfg, bool on, bool early, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { parry36 = on }, 3602, out Vec3 sp, out double yaw, out double ux, out double uz);
        double reach = cfg.Num("orb.reflectRange");
        Husk sh = Core36Spawn(sim, "spitter", sp, ux, uz, 12, 0);
        if (!(sh is Spitter s)) { fails.Add("parry: no spitter placed"); return "parry: no spitter"; }
        Vec3 pin = s.position;
        int parries36 = 0, parriesWeb = 0;
        string weapon36 = "";
        double hitstop = 0;
        sim.events.On<HitEvent>(Parry36.Event, e => { parries36++; weapon36 = e.weapon; });
        sim.events.On<HitEvent>("parry", e => { if (e.owner == "player") parriesWeb++; });
        bool thrown = false, punched = false;
        int punchStep = -1, globInReachStep = -1;
        double lead = early ? 0.08 : 0;
        for (int i = 0; i < MovementMath.StepsFor(3, dt); i++)
        {
            if (s.alive)
            {
                s.position = pin;
                s.velocity = default;
                s.cooldownSteps = 9999; // its own volleys never start: the harness throws one
                if (s.state == "tell") s.state = "idle";
            }
            if (!thrown && i == MovementMath.StepsFor(0.2, dt))
            {
                HuskAi saved = s.ai;
                s.ai = null;
                sim.husks.acid.ThrowVolley(s, sim.mover.position);
                s.ai = saved;
                thrown = true;
            }
            double nearest = double.PositiveInfinity;
            foreach (SpitterAcid.Glob g in sim.husks.acid.globs) if (g.alive && !g.reflected) nearest = Math.Min(nearest, Vec3.DistanceTo(g.position, sim.Eye));
            if (globInReachStep < 0 && nearest <= reach) globInReachStep = i;
            double closing = cfg.Num("spitter.globSpeed");
            CombatInput c = NoCombat36;
            if (thrown && !punched && nearest <= reach + closing * lead && nearest > (early ? reach : -1))
            {
                c = Punch36;
                punched = true;
                punchStep = i;
            }
            (double yw, double py) = ArclineLook(sim.Eye, s.AimPoint);
            Core36Tick(sim, yw, py, c);
            if (sim.hitstopLeft > hitstop) hitstop = sim.hitstopLeft;
        }
        Parry36 p = sim.parry36Core;
        if (Environment.GetEnvironmentVariable("CORE36_DEBUG") == "1" && p != null) Console.WriteLine($"    burst at {p.lastBurst} spitter {s.AimPoint} player {sim.mover.position} hits {p.burstHits} ray {(sim.world.RayIntersect(sim.Eye, Vec3.Normalize(s.AimPoint - sim.Eye), out RayHit rh) ? rh.distance.ToString("0.00") + " n " + rh.normal : "clear")} dist {Vec3.DistanceTo(sim.Eye, s.AimPoint):0.00} core {(sim.core != null)}");
        string tag = early ? "buffer" : "parry ";
        string line = $"{tag} {(on ? "ON " : "OFF")}: punch at step {punchStep}, first glob in reach at step {globInReachStep} ({(punchStep >= 0 && globInReachStep >= 0 ? ((globInReachStep - punchStep) * dt).ToString("0.000") + " s early" : "-")}); parry (web) {parriesWeb}, parry36 {parries36} [{weapon36}], returns {(on ? p.globReturns : 0)}, bursts {(on ? p.bursts : 0)} (kills {(on ? p.burstKills : 0)}), spitter {(s.alive ? $"ALIVE {s.hp:0} hp" : "DEAD")}, freeze seen {hitstop:0.00} s";
        if (on)
        {
            if (parries36 != 1) fails.Add($"{tag} ON: parry36 {parries36}, want 1");
            if (early && weapon36 != "parry36buffer") fails.Add($"{tag} ON: not made by the buffer ({weapon36})");
            if (p.bursts < 1) fails.Add($"{tag} ON: no return burst");
            if (s.alive) fails.Add($"{tag} ON: the spitter lived ({s.hp:0} hp)");
            if (hitstop < cfg.Num("design.n36.parry36.freezeS") - 1e-9) fails.Add($"{tag} ON: freeze {hitstop:0.00} s");
        }
        else
        {
            if (parries36 != 0) fails.Add($"{tag} OFF: a parry36");
            if (early && parriesWeb != 0) fails.Add($"{tag} OFF: the early punch parried");
            if (!early && s.alive && s.hp <= 0) fails.Add($"{tag} OFF: odd hp");
        }
        return line;
    }

    // ---- launch / two kills ----
    static string Core36Launch(GameConfig cfg, bool on, bool dash, bool second, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { launch36 = on }, 3603, out Vec3 sp, out double yaw, out double ux, out double uz, 15);
        double hp = second ? cfg.Num("husk.hp") : 1e9;
        Husk a = Core36Spawn(sim, "husk", sp, ux, uz, dash ? 6 : 1.6, hp);
        Husk b = second ? Core36Spawn(sim, "husk", sp, ux, uz, dash ? 10.5 : 6, hp) : null;
        if (a == null || (second && b == null)) { fails.Add($"launch: husks not placed (a {(a != null)}, b {(b != null)})"); return "launch: no husks"; }
        Vec3 pinA = a.position, pinB = b != null ? b.position : default;
        int kills = 0, bursts = 0;
        sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; });
        sim.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => bursts++);
        bool punched = false;
        Vec3 at = default;
        double thrown = 0;
        string verbAtPunch = "";
        double pRange = cfg.Num("punch.range");
        for (int i = 0; i < MovementMath.StepsFor(2.5, dt); i++)
        {
            if (!punched && a.alive) { a.position = pinA; a.velocity = default; a.cooldownSteps = 9999; }
            if (b != null && b.alive) { b.position = pinB; b.velocity = default; b.cooldownSteps = 9999; b.knockSteps = 0; }
            var f = new InputFrame { moveZ = dash && !punched ? 1 : 0, dash = new Button { held = dash && i < 3, pressed = dash && i == 1 } };
            CombatInput c = NoCombat36;
            double flat = Math.Sqrt((a.position.x - sim.mover.position.x) * (a.position.x - sim.mover.position.x) + (a.position.z - sim.mover.position.z) * (a.position.z - sim.mover.position.z));
            if (!punched && i > 2 && flat <= pRange * 0.9)
            {
                c = Punch36;
                punched = true;
                at = a.position;
                verbAtPunch = sim.mover.state.ToString();
            }
            Core36Tick(sim, yaw, 0, c, f);
            if (punched && a.alive) thrown = Math.Max(thrown, Math.Sqrt((a.position.x - at.x) * (a.position.x - at.x) + (a.position.z - at.z) * (a.position.z - at.z)));
        }
        Launch36 l = sim.launch36Core;
        string tag = second ? "twokill" : dash ? "launch " : "stand  ";
        string line = $"{tag} {(on ? "ON " : "OFF")}: punched {(punched ? "yes" : "NO")} in state {verbAtPunch}, launches {(on ? l.launches : 0)}, thrown {thrown:0.0} m, bursts {bursts}{(on ? $" (wall {l.wallBursts}, landed {l.landed}, end {l.lastEnd}, gap {l.lastMinGap:0.00} m)" : "")}, kills {kills}";
        if (!punched) fails.Add($"{tag} {(on ? "ON" : "OFF")}: the punch never landed");
        if (on && dash && l.launches != 1) fails.Add($"{tag} ON: launches {l.launches}, want 1");
        if (on && !dash && l.launches != 0) fails.Add($"{tag} ON: a standing punch launched");
        if (on && dash && !second && thrown < 4) fails.Add($"{tag} ON: thrown {thrown:0.0} m, want >= 4");
        if (on && second && (kills != 2 || bursts < 1)) fails.Add($"{tag} ON: kills {kills}, bursts {bursts}, want 2 and >= 1");
        if (!on && (bursts != 0 || (second && kills != 0))) fails.Add($"{tag} OFF: bursts {bursts}, kills {kills}");
        return line;
    }

    // ---- heal ----
    static string Core36Heal(GameConfig cfg, bool on, double d, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { heal36 = on }, 3604, out Vec3 sp, out double yaw, out double ux, out double uz);
        Husk h = Core36Spawn(sim, "husk", sp, ux, uz, d, 1);
        if (h == null) { fails.Add($"heal {d} m: no husk"); return "heal: no husk"; }
        Vec3 pin = h.position;
        int heals = 0;
        double amount = 0;
        sim.events.On<Heal36Event>(Heal36.Event, e => { heals++; amount += e.amount; });
        if (Environment.GetEnvironmentVariable("CORE36_DEBUG") == "1")
        {
            sim.events.On<ShotEvent>("shotResult", e => Console.WriteLine($"    shot {e.weapon} {e.surface} end {e.end} husk {h.AimPoint} weapon {sim.loadout.Weapon} owned [{string.Join(",", sim.loadout.owned)}]"));
            sim.events.On<HitEvent>("hit", e => Console.WriteLine($"    hit {e.weapon} {e.damage}"));
            sim.events.On("fire", o => Console.WriteLine($"    fire {o}"));
            Console.WriteLine($"    owned [{string.Join(",", sim.loadout.owned)}] weapon {sim.loadout.Weapon} ready {sim.loadout.ready} run {sim.run.state} husk {h.position} hp {h.hp}");
        }
        sim.health.hp = 50;
        double hpAtKill = -1;
        sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") hpAtKill = sim.health.hp; });
        for (int i = 0; i < MovementMath.StepsFor(1.0, dt); i++)
        {
            if (h.alive) { h.position = pin; h.velocity = default; h.cooldownSteps = 9999; }
            (double yw, double py) = h.alive ? ArclineLook(sim.Eye, h.AimPoint) : (yaw, 0);
            int ph = i % 30; // taps (charge35 fires a tap on its release): 3 steps held, then released
            bool fire = h.alive && i > 5 && ph < 3;
            Core36Tick(sim, yw, py, new CombatInput { weapon = -1, fire = new Button { held = fire, pressed = fire && ph == 0 } });
        }
        double want = cfg.Num("design.n36.heal36.amount");
        bool near = d <= cfg.Num("design.n36.heal36.radius");
        string line = $"heal {d,4:0.0} m {(on ? "ON " : "OFF")}: killed {(!h.alive ? "yes" : "NO")}, heal36 {heals} (+{amount:0}), hp 50 -> {sim.health.hp:0}";
        if (h.alive) fails.Add($"heal {d} m {(on ? "ON" : "OFF")}: the husk lived");
        if (on && near && (heals != 1 || Math.Abs(amount - want) > 1e-9)) fails.Add($"heal {d} m ON: heal36 {heals} +{amount}, want 1 +{want}");
        if ((!on || !near) && heals != 0) fails.Add($"heal {d} m {(on ? "ON" : "OFF")}: healed");
        return line;
    }

    // ---- kinds: a drone 12 m, a charger 12 m, a lancer 20 m (bags): their tells (drone YELLOW droneShot, charger RED, lancer RED) ----
    static string Core36Kinds(GameConfig cfg, bool on, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { tells36 = on }, 3605, out Vec3 sp, out double yaw, out double ux, out double uz);
        var seen = new Dictionary<string, string>();
        sim.events.On<Tell36>(Tells36.Event, t => { if (!seen.ContainsKey(t.attack)) seen[t.attack] = $"{t.kind} {t.windupSteps * dt:0.00}s"; });
        int droneCharge = -1, chargerTell = -1;
        Husk d = Core36Spawn(sim, "drone", sp, ux, uz, 12, 1e9);
        Husk c = Core36Spawn(sim, "charger", sp, -uz, ux, 12, 1e9);
        Husk l = Core36Spawn(sim, "lancer", sp, uz, -ux, 14, 1e9);
        for (int i = 0; i < MovementMath.StepsFor(6, dt); i++)
        {
            if (d is Drone dr && dr.chargeSteps > 0 && droneCharge < 0) droneCharge = dr.chargeSteps;
            if (c is Charger ch && ch.state == "tell" && chargerTell < 0) chargerTell = ch.stateSteps;
            Core36Tick(sim, yaw, 0, NoCombat36);
        }
        string Got(string a) => seen.TryGetValue(a, out string v) ? v : "none";
        string line = $"kinds  {(on ? "ON " : "OFF")}: placed drone {d != null} charger {c != null} lancer {l != null}; droneShot {Got(SurvivalSim.Tell36DroneShot)}, chargerCharge {Got(SurvivalSim.Tell36ChargerCharge)}, lancerBeam {Got(SurvivalSim.Tell36LancerBeam)}; the drone's charge at its start {(droneCharge < 0 ? "none" : (droneCharge * dt).ToString("0.00") + " s")}, the charger's tell {(chargerTell < 0 ? "none" : (chargerTell * dt).ToString("0.00") + " s")}";
        if (on)
        {
            if (d != null && Got(SurvivalSim.Tell36DroneShot) != $"Yellow {MovementMath.StepsFor(cfg.Num("design.n36.tells36.droneChargeS"), dt) * dt:0.00}s") fails.Add("kinds ON: droneShot");
            if (c != null && Got(SurvivalSim.Tell36ChargerCharge) != $"Red {MovementMath.StepsFor(cfg.Num("design.n36.tells36.chargerTellS"), dt) * dt:0.00}s") fails.Add("kinds ON: chargerCharge");
            if (l != null && !Got(SurvivalSim.Tell36LancerBeam).StartsWith("Red")) fails.Add("kinds ON: lancerBeam");
        }
        else if (seen.Count > 0) fails.Add("kinds OFF: a tell");
        return line;
    }

    // ---- a drone orb parried: back at the drone, a burst, the drone (30 hp) dead ----
    static string Core36Orb(GameConfig cfg, bool on, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { parry36 = on }, 3606, out Vec3 sp, out double yaw, out double ux, out double uz);
        Husk d = Core36Spawn(sim, "drone", sp, ux, uz, 10, 0);
        if (!(d is Drone dr)) { fails.Add("orb: no drone"); return "orb: no drone"; }
        double reach = cfg.Num("orb.reflectRange");
        int parries36 = 0;
        sim.events.On<HitEvent>(Parry36.Event, e => parries36++);
        Vec3 pin = dr.position;
        bool fired = false, punched = false;
        for (int i = 0; i < MovementMath.StepsFor(3, dt); i++)
        {
            if (dr.alive) { dr.position = pin; dr.velocity = default; dr.fireSteps = 9999; dr.chargeSteps = 0; }
            if (!fired && i == 10)
            {
                Vec3 to = sim.Eye - dr.position;
                sim.husks.orbs.Fire(dr.position + Vec3.Normalize(to) * 1.0, Vec3.Normalize(to), dr);
                fired = true;
            }
            double nearest = double.PositiveInfinity;
            foreach (DroneOrbs.Orb o in sim.husks.orbs.pool) if (o.alive && !o.reflected) nearest = Math.Min(nearest, Vec3.DistanceTo(o.position, sim.Eye));
            CombatInput c = NoCombat36;
            if (fired && !punched && nearest <= reach) { c = Punch36; punched = true; }
            (double yw, double py) = ArclineLook(sim.Eye, dr.AimPoint);
            Core36Tick(sim, yw, py, c);
        }
        Parry36 p = sim.parry36Core;
        string line = $"orb    {(on ? "ON " : "OFF")}: punched {punched}, parry36 {parries36}, orb returns {(on ? p.orbReturns : 0)}, bursts {(on ? p.bursts : 0)}, drone {(dr.alive ? $"ALIVE {dr.hp:0} hp" : "DEAD")}";
        if (!punched) fails.Add($"orb {(on ? "ON" : "OFF")}: no punch");
        if (on && (parries36 != 1 || p.orbReturns != 1 || p.bursts < 1 || dr.alive)) fails.Add("orb ON: not returned + burst + drone dead");
        if (!on && parries36 != 0) fails.Add("orb OFF: parry36");
        return line;
    }

    // ---- the husk buffer before the wind-up: a husk 1.4 m away whose cooldown ends in `cool` steps; one punch ----
    static string Core36Early(GameConfig cfg, bool on, int cool, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { parry36 = on }, 3607, out Vec3 sp, out double yaw, out double ux, out double uz);
        Husk h = Core36Spawn(sim, "husk", sp, ux, uz, 1.4, 1e9);
        if (h == null) { fails.Add("early: no husk"); return "early: no husk"; }
        int parries36 = 0, parriesWeb = 0;
        string w = "";
        sim.events.On<HitEvent>(Parry36.Event, e => { parries36++; w = e.weapon; });
        sim.events.On<HitEvent>("parry", e => { if (e.owner == "player") parriesWeb++; });
        Vec3 pin = h.position;
        for (int i = 0; i < 40; i++)
        {
            h.cooldownSteps = 9999;
            Core36Tick(sim, yaw, 0, NoCombat36);
        }
        h.cooldownSteps = cool;
        (double yw, double py) = ArclineLook(sim.Eye, h.AimPoint);
        int hits = 0;
        sim.events.On<MeleeEvent>("punch", e => hits += e.hits);
        string pre = $"grounded {h.grounded} leaping {h.leaping} stagger {h.staggerSteps} knock {h.knockSteps} wind {h.windupSteps} ai {(h.ai == null ? "none" : h.ai.attack.ToString())} dist {Vec3.DistanceTo(h.position, sim.mover.position):0.00} type {h.GetType().Name}";
        Core36Tick(sim, yw, py, Punch36);
        for (int i = 0; i < 30; i++) Core36Tick(sim, yw, py, NoCombat36);
        int buffer = sim.parry36Core != null ? sim.parry36Core.bufferSteps : MovementMath.StepsFor(cfg.Num("design.n36.parry36.bufferS"), dt);
        bool want = on && cool <= buffer;
        string line = $"early  {(on ? "ON " : "OFF")} cooldown {cool,2} steps (buffer {buffer}): parry (web) {parriesWeb}, parry36 {parries36} [{w}], husk staggered {h.staggerSteps > 0}, punch hits {hits} | {pre}";
        if (want && (parries36 != 1 || w != "parry36buffer")) fails.Add($"early ON {cool}: no buffered parry");
        if (!want && (parries36 != 0 || parriesWeb != 0)) fails.Add($"early {(on ? "ON" : "OFF")} {cool}: a parry");
        return line;
    }

    // ---- n36-r2's parry36 RED reproduced: a spitter put in its tell (as the reel does), the enemies frozen (FeelReel.One sets husks.frozen
    // before every phase-J body) vs ticking: frozen, the sim never counts the tell (no volley); ticking, it fires at the tell's end ----
    static string Core36Frozen(GameConfig cfg, bool frozen, List<string> fails)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        SurvivalSim sim = Core36Sim(cfg, new N36Blocks { tells36 = true }, 3608, out Vec3 sp, out double yaw, out double ux, out double uz);
        if (!(Core36Spawn(sim, "spitter", sp, ux, uz, 10, 1e9) is Spitter s)) { fails.Add("frozen: no spitter"); return "frozen: no spitter"; }
        int v0 = sim.husks.acid.volleys;
        int tell = MovementMath.StepsFor(sim.husks.acid.p.tellTime, dt);
        s.state = "tell";
        s.stateSteps = s.tellSteps = tell;
        s.committed = true;
        sim.husks.frozen = frozen;
        for (int i = 0; i < tell + 30; i++) Core36Tick(sim, yaw, 0, NoCombat36);
        int volleys = sim.husks.acid.volleys - v0;
        string line = $"reel setup, enemies {(frozen ? "FROZEN " : "ticking")}: the tell counted {tell - Math.Max(0, s.stateSteps)} / {tell} steps, state {s.state}, volleys {volleys}, live tell {(sim.tells36.Of(s.id) != null)}";
        if (frozen ? volleys != 0 : volleys != 1) fails.Add($"frozen repro ({frozen}): volleys {volleys}");
        return line;
    }

    static void N36CoreCommand(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        var fails = new List<string>();
        Console.WriteLine("n36core: tells36, parry36, launch36, heal36 each ON (forced) then OFF");
        foreach (bool on in new[] { true, false })
        {
            Console.WriteLine("  " + Core36Tells(cfg, on, fails));
            Console.WriteLine("  " + Core36Parry(cfg, on, false, fails));
            Console.WriteLine("  " + Core36Parry(cfg, on, true, fails));
            Console.WriteLine("  " + Core36Launch(cfg, on, true, false, fails));
            Console.WriteLine("  " + Core36Launch(cfg, on, false, false, fails));
            Console.WriteLine("  " + Core36Launch(cfg, on, true, true, fails));
            Console.WriteLine("  " + Core36Heal(cfg, on, 2.5, fails));
            Console.WriteLine("  " + Core36Heal(cfg, on, 12, fails));
            Console.WriteLine("  " + Core36Kinds(cfg, on, fails));
            Console.WriteLine("  " + Core36Orb(cfg, on, fails));
            Console.WriteLine("  " + Core36Early(cfg, on, 8, fails));
            Console.WriteLine("  " + Core36Early(cfg, on, 40, fails));
        }
        Console.WriteLine("  " + Core36Frozen(cfg, true, fails));
        Console.WriteLine("  " + Core36Frozen(cfg, false, fails));
        Console.WriteLine(fails.Count == 0 ? "n36core GREEN" : $"n36core RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
