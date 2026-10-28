// NIGHT35 builder W (weapons): the W rows' headless judges. ONE entry method, N35Weapons(cmd, args, ref code) (declared and called in Program.cs).
//   n35charge [--off35]   design.n35.charge35 (DECIDE35 D-8) per gun (KESTREL, THUNDERCLAP), scripted RT at a husk 4 m ahead (design.n31.charge on, the
//                         n27 row off, the other rows from the config):
//                         HOLD to full + 0.1 s, release: normal shots BEFORE the charged one (want 0), the charged shot (1), charge35Start / Full / Fire
//                         (1 / 1 / 1), the charged shot's damage vs one normal shot's, and the charge's cooldown left (want charge35's cooldownS less the
//                         time since);
//                         TAP (3 steps): one normal shot at the release, no charge;
//                         HELD 4 s at husks with their real hit points: the full charge fires itself (holdMaxS) and the hold keeps killing after 1.5 s.
//                         GREEN = every PASS line with charge35 on. `--off35`: NIGHT31's rule (0.4 s of auto-fire before the charge): the "no shots first"
//                         line is RED (the RED side of L-014).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N35Weapons(string cmd, string[] args, ref int code)
    {
        if (cmd == "n35charge") N35ChargeCommand(args, ref code);
        else if (cmd == "n35gauntlet") N35GauntletCommand(args, ref code);
    }

    //   n35gauntlet [--off35]   the lead's check for verbs35 row 28: a SURVIVAL run owning [revolver, gauntlet] with design.n35.charge35 on (off with --off35):
    //                           swap to the gauntlet (wait 0.6 s), ONE RT press (3 steps), 1 s, ONE LT press (3 steps), 1 s, a bag husk 2 m ahead. Prints every
    //                           event name emitted in each window (EventBus.Emitted's deltas) so the reel counts the right one.
    static void N35GauntletCommand(string[] args, ref int code)
    {
        bool on = !N35Blocks.ForceOff;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n35Override = new N35Blocks { charge35 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3503, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 2);
        Vec3 sp = map.playerSpawn;
        Husk body = null;
        var snap = new Dictionary<string, int>();
        void Snap() { snap.Clear(); foreach (var kv in sim.events.Emitted) snap[kv.Key] = kv.Value; }
        string Delta()
        {
            var parts = new List<string>();
            foreach (var kv in sim.events.Emitted) { snap.TryGetValue(kv.Key, out int was); if (kv.Value > was) parts.Add($"{kv.Key} {kv.Value - was}"); }
            parts.Sort(StringComparer.Ordinal);
            return string.Join(", ", parts);
        }
        void Run(int n, Func<int, CombatInput> input)
        {
            for (int s = 0; s < n; s++)
            {
                body = Pin27(sim, body, sp, ux, uz, 2.0, 1e9);
                double py = 0;
                if (body != null && body.alive) (yaw, py) = ArclineLook(sim.Eye, body.AimPoint);
                Tick27(sim, yaw, py, input(s));
            }
        }
        Run(Steps(0.6), s => new CombatInput { weapon = s == 0 ? slot : -1 });
        Console.WriteLine($"n35gauntlet: charge35 {(on ? "ON" : "OFF (--off35)")}; owned [{string.Join(", ", sim.loadout.owned)}], in hand {sim.loadout.Weapon}");
        Snap();
        Run(Steps(1.0), s => new CombatInput { weapon = -1, fire = new Button { pressed = s == 0, held = s < 3 } });
        string rt = Delta();
        Console.WriteLine($"  RT press: {rt}");
        Snap();
        Run(Steps(1.0), s => new CombatInput { weapon = -1, altFire = new Button { pressed = s == 0, held = s < 3 } });
        Console.WriteLine($"  LT press: {Delta()}");
        bool ok = rt.Contains("meleeSwing") || rt.Contains("fire ");
        Console.WriteLine(ok ? "n35gauntlet GREEN (the RT press swings the gauntlet)" : "n35gauntlet RED (no swing on the RT press)");
        code = ok ? 0 : 1;
    }

    sealed class Charge35Result
    {
        public int fires, charged, chargedHits, starts, fulls, fires35, kills, firesLate, killsLate, autoReleased, cooldownAtEnd, firstFire = -1, firstCharged = -1;
        public int normalBefore; // "fire" events of the gun before the first charged shot
        public double damage, chargedDamage;
    }

    static Charge35Result Charge35Run(GameConfig cfg, SurvivalMap map, string gun, bool on, Func<int, bool> held, int steps, bool realHp, int lateFrom, int pressSteps = 1)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { chargeShots27 = false };
        sim.n31Override = new N31Blocks { charge = true };
        sim.n35Override = new N35Blocks { charge35 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3502, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        var r = new Charge35Result();
        int s = 0;
        var fireSteps = new List<int>();
        sim.events.On<string>("fire", w => { if (w != gun) return; r.fires++; fireSteps.Add(s); if (r.firstFire < 0) r.firstFire = s; if (s >= lateFrom) r.firesLate++; });
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { r.charged++; r.chargedHits += e.hits; if (r.firstCharged < 0) r.firstCharged = s; });
        sim.events.On<ChargeShot27Event>("charge35Start", e => r.starts++);
        sim.events.On<ChargeShot27Event>("charge35Full", e => r.fulls++);
        sim.events.On<ChargeShot27Event>("charge35Fire", e => r.fires35++);
        sim.events.On<HitEvent>("hit", h => { if (h.owner == "player" && h.weapon == gun) r.damage += h.damage; });
        Husk body = null;
        double dmgBefore = 0;
        int downFor = 0; // steps the trigger has been down before this one
        for (s = 0; s < steps; s++)
        {
            double py = 0;
            if (realHp)
            {
                if (body != null && !body.alive) { r.kills++; if (s >= lateFrom) r.killsLate++; body = null; }
                if (body == null) body = sim.husks.Spawn("husk", new Vec3(sp.x + ux * 4, sp.y, sp.z + uz * 4), sp);
                if (body != null) body.position = new Vec3(sp.x + ux * 4, sp.y, sp.z + uz * 4);
            }
            else body = Pin27(sim, body, sp, ux, uz, 4.0, 1e9);
            if (body != null && body.alive) (yaw, py) = ArclineLook(sim.Eye, body.AimPoint);
            bool down = held(s), prev = s > 0 && held(s - 1);
            downFor = down ? (prev ? downFor + 1 : 0) : 0;
            int chargedBefore = r.charged;
            dmgBefore = r.damage;
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && (!prev || downFor < pressSteps) } });
            if (r.charged > chargedBefore) r.chargedDamage += r.damage - dmgBefore; // the charged shot's own step
        }
        foreach (int f in fireSteps) if (r.firstCharged < 0 || f < r.firstCharged) r.normalBefore++; // the charged shot's own "fire" is on its step
        if (r.firstCharged < 0) r.normalBefore = r.fires;
        ChargeShots27 c = sim.loadout.charge27;
        if (c != null)
        {
            r.autoReleased = c.autoReleased;
            r.cooldownAtEnd = c.cooldown[ChargeShots27.Index(gun)];
        }
        return r;
    }

    static void N35ChargeCommand(string[] args, ref int code)
    {
        bool on = !N35Blocks.ForceOff;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var fails = new List<string>();
        void Check(bool ok, string what)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} {what}");
            if (!ok) fails.Add(what);
        }
        Console.WriteLine($"n35charge: {(on ? "design.n35.charge35 ON (forced on here; the row's own enabled is " + cfg.Design("n35.charge35.enabled") + ")" : "--off35: NIGHT31's rule (the RED side)")}; design.n31.charge on, n33.pierce33 {cfg.Design("n33.pierce33.enabled")}");
        foreach (string gun in new[] { "revolver", "shotgun" })
        {
            string name = gun == "revolver" ? "KESTREL" : "THUNDERCLAP";
            double fullS = cfg.Num("design.n31.charge." + gun + ".fullSeconds");
            double holdS = on ? 0 : cfg.Num("design.n31.charge.holdSeconds"); // off: the hold threshold comes before the charge
            double cdS = on ? cfg.Num("design.n35.charge35." + gun + ".cooldownS") : cfg.Num("design.n31.charge." + gun + ".cooldownSeconds");
            int lead = Steps(0.5);
            // 1. hold to full (+ 0.1 s), release
            int fullEnd = lead + Steps(holdS) + Steps(fullS) + Steps(0.1);
            var full = Charge35Run(cfg, map, gun, on, s => s >= lead && s < fullEnd, fullEnd + Steps(0.2), false, int.MaxValue);
            // n35-r3: the same hold with `pressed` repeated on its first 3 steps (a forced input applied to every step of a rendered frame: verbs35)
            var full3 = Charge35Run(cfg, map, gun, on, s => s >= lead && s < fullEnd, fullEnd + Steps(0.2), false, int.MaxValue, 3);
            // 2. a tap: 3 steps down
            var tap = Charge35Run(cfg, map, gun, on, s => s >= lead && s < lead + 3, lead + Steps(1), false, int.MaxValue);
            // 3. held 4 s at husks with real hit points (the smoke's virtual pad, the bots)
            int late = lead + Steps(1.5);
            var hold = Charge35Run(cfg, map, gun, on, s => s >= lead && s < lead + Steps(4), lead + Steps(4), true, late);
            double ratio = tap.damage > 0 ? full.chargedDamage / tap.damage : 0;
            Console.WriteLine($" {name} ({gun}): full {fullS} s from the {(on ? "press" : "0.4 s hold")}, charge cooldown {cdS} s");
            Console.WriteLine($"  HOLD {holdS + fullS + 0.1:0.00} s + release: {full.normalBefore} normal shots before the charged one; charged {full.charged} ({full.chargedHits} hit, at +{(full.firstCharged >= 0 ? full.firstCharged - lead : -1)} steps); charge35 Start / Full / Fire {full.starts} / {full.fulls} / {full.fires35}; charged damage {full.chargedDamage:0.0} vs a tap's {tap.damage:0.0} = x{ratio:0.00}; charge cooldown left {full.cooldownAtEnd} steps");
            Console.WriteLine($"  TAP (3 steps): {tap.fires} shot(s), first at +{(tap.firstFire >= 0 ? tap.firstFire - lead : -1)} steps (the release is +3), charged {tap.charged}, charge35Start {tap.starts}");
            Console.WriteLine($"  HELD 4 s (real husks): {hold.fires} shots, {hold.kills} kills; after 1.5 s {hold.firesLate} shots / {hold.killsLate} kills; charged {hold.charged} (fired itself {hold.autoReleased})");
            Check(full.normalBefore == 0, $"{name}: a hold charges with NO normal shots first ({full.normalBefore})");
            Check(full3.normalBefore == 0 && full3.charged == 1, $"{name}: pressed repeated on 3 steps is ONE press ({full3.normalBefore} normal shots first, {full3.charged} charged)");
            Check(full.charged == 1 && full.chargedHits >= 1, $"{name}: the release of a full charge fires ONE charged shot that hits ({full.charged}, {full.chargedHits} hit)");
            if (on) Check(full.starts == 1 && full.fulls == 1 && full.fires35 == 1, $"{name}: charge35Start / Full / Fire once each");
            Check(ratio > (gun == "revolver" ? 4.0 : 1.5), $"{name}: the charged shot outdamages a normal shot (x{ratio:0.00})");
            Check(full.cooldownAtEnd == Steps(cdS) - Steps(0.2), $"{name}: the charge's cooldown is {cdS} s ({full.cooldownAtEnd} left = {Steps(cdS) - Steps(0.2)} want)");
            Check(tap.fires == 1 && tap.charged == 0 && tap.starts == 0, $"{name}: a tap fires ONE normal shot and no charge");
            Check(hold.charged >= 1 && hold.autoReleased >= 1 && hold.killsLate > 0, $"{name}: a held trigger never stalls (charged {hold.charged}, fired itself {hold.autoReleased}, kills after 1.5 s {hold.killsLate})");
        }
        Console.WriteLine(fails.Count == 0 ? "n35charge GREEN" : $"n35charge RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
