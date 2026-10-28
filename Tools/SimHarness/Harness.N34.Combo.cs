// NIGHT34 builder B (leftover 3, design.n34.combo34): the named bend combo's headless judge (the entry is N34Bend in Harness.N34.Bend.cs).
//   combo34   the SURVIVAL map, the round clock held, invulnerable; the KESTREL, the THUNDERCLAP and the NAILGUN owned; three 1 hp husks pinned 5 / 7 / 9 m
//             ahead in an open direction. One kill per gun in that order (the slot asked, then the trigger held on the nearest live husk until it dies).
//             Runs: A a bend (the injected latch, Bend34LatchForCheck: a FULL bend from the starting meter), combo34 on; B the same, combo34 off; C no
//             bend, combo34 on (the kills outside any bend).
//             GREEN: A: 3 kills inside the bend, bend34Combo twice (2 guns: names[0], 3 guns: names[1]) with the style bonus paid; B and C: none.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Combo34Result
    {
        public int kills, killsInside, combos;
        public double comboPoints;
        public readonly List<string> names = new List<string>();
        public string why = "";
    }

    static Combo34Result Combo34Run(GameConfig cfg, bool comboOn, bool bend, uint seed)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var r = new Combo34Result();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n34Override = new N34Blocks { bend34 = true, bendSplit34 = true, combo34 = comboOn }; // split: the bend is 2.5 s of player steps (global: 75 sim steps, short for three guns in sim time)
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.holdRunClock = true;
        sim.mover.invulnerable = true;
        string[] guns = { "revolver", "shotgun", "nailgun" };
        foreach (string g in guns) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        Vec3 sp = sim.map.playerSpawn;
        sim.Respawn(sp, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, sp, 15);
        var husks = new List<Husk>();
        var pins = new List<Vec3>();
        foreach (double d in new[] { 5.0, 7.0, 9.0, 6.0, 8.0 })
        {
            if (husks.Count == 3) break;
            Husk h = null;
            for (int k = 0; k < 6 && h == null; k++) h = sim.husks.Spawn("husk", new Vec3(sp.x + ux * d, sp.y + 0.25 * k, sp.z + uz * d), sp);
            if (h == null) continue;
            h.hp = h.maxHp = 1;
            husks.Add(h);
            pins.Add(h.position);
        }
        if (husks.Count < 3) { r.why = $"husks placed {husks.Count} / 3"; return r; }
        sim.events.On<KillEvent>("kill", e => { if (e.owner != "player") return; r.kills++; if (sim.bend34.Inside) r.killsInside++; });
        sim.events.On<Bend34ComboEvent>("bend34Combo", e => { r.combos++; r.comboPoints += e.points; r.names.Add($"{e.name} ({e.guns} guns: {e.gunList}, {e.kills} kills, +{e.points:0})"); });
        void Tick(CombatInput c)
        {
            for (int i = 0; i < husks.Count; i++) if (husks[i].alive) { husks[i].position = pins[i]; husks[i].velocity = default; }
            Husk aim = husks.Find(x => x.alive);
            double py = 0;
            if (aim != null) (yaw, py) = ArclineLook(sim.Eye, aim.AimPoint);
            Tick27(sim, yaw, py, c);
        }
        for (int s = 0; s < Steps(0.5); s++) Tick(new CombatInput { weapon = -1 });
        if (bend)
        {
            sim.Bend34LatchForCheck();
            for (int s = 0; s < 2; s++) Tick(new CombatInput { weapon = -1 });
        }
        foreach (string g in guns)
        {
            int slot = sim.loadout.owned.IndexOf(g), kills0 = r.kills;
            for (int s = 0; s < Steps(1.0) && sim.loadout.Weapon != g; s++) Tick(new CombatInput { weapon = s == 0 ? slot : -1 });
            for (int s = 0; s < Steps(bend ? 0.3 : 1.0) && sim.loadout.Weapon == g && !sim.loadout.ready; s++) Tick(new CombatInput { weapon = -1 });
            for (int s = 0; s < Steps(1.5) && r.kills == kills0; s++) Tick(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = s == 0 } });
        }
        return r;
    }

    static void Combo34Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        Combo34Rules rules = Combo34Rules.From(cfg);
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Console.WriteLine($"combo34: names [{string.Join(", ", rules.names)}], style points [{string.Join(", ", rules.stylePoints)}]; bend34 (split) forced on, combo34 per run");
        Combo34Result a = Combo34Run(cfg, true, true, 3430), b = Combo34Run(cfg, false, true, 3430), c = Combo34Run(cfg, true, false, 3430);
        foreach (var (tag, r) in new[] { ("A bend, combo on ", a), ("B bend, combo off", b), ("C no bend, on    ", c) })
            Console.WriteLine($"  {tag}: kills {r.kills} (inside a bend {r.killsInside}), bend34Combo {r.combos} [{string.Join("; ", r.names)}], combo style {r.comboPoints:0}{(r.why.Length > 0 ? " | " + r.why : "")}");
        Need(a.kills == 3 && a.killsInside == 3, "A: 3 kills inside the bend");
        Need(a.combos == 2 && a.names.Count == 2 && a.names[0].StartsWith(rules.names[0]) && a.names[1].StartsWith(rules.names[1]) && a.comboPoints > 0, "A: two combos (2 guns, 3 guns) with the bonus paid");
        Need(b.kills == 3 && b.combos == 0, "B: combo34 off: no combo");
        Need(c.kills == 3 && c.killsInside == 0 && c.combos == 0, "C: no bend: no combo");
        Console.WriteLine(fails.Count == 0 ? "combo34 GREEN" : $"combo34 FAIL: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    // ---- the REA addendum (research/rea-ultrakill.md): rea34 runs three checks, each BEFORE (combo34 off) and AFTER (combo34 on, its sub-switch as the
    // config has it), no bend in any of them:
    //   arsenal34    a 40 hp husk pinned 6 m ahead: one KESTREL shot (25), a swap to the THUNDERCLAP, its shot kills: arsenal34 events and the style gained.
    //   quickdraw34  a bag husk: a swap to the THUNDERCLAP, the trigger held from the raise (a hit inside quickdrawS), then a second swap back with the
    //                trigger only 0.5 s after the raise: quickdraw34 events (want 1 after, 0 before).
    //   whipready34  the whip rig (ARCLINE, a bag charger 10 m out): a punch, then at once a grapple tap at the charger: the punch cooldown left at the
    //                whip33Latch step, and the steps from the latch until a punch can land again (want <= whipReadyS after).

    static SurvivalSim Rea34Sim(GameConfig cfg, bool on, uint seed, out double yaw, out double ux, out double uz)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n34Override = new N34Blocks { bend34 = true, combo34 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.holdRunClock = true;
        sim.mover.invulnerable = true;
        foreach (string g in new[] { "revolver", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        Vec3 sp = sim.map.playerSpawn;
        sim.Respawn(sp, 0);
        (yaw, ux, uz) = ArclineOpenDir(sim, sp, 15);
        return sim;
    }

    static Husk Rea34Husk(SurvivalSim sim, double ux, double uz, double hp)
    {
        Vec3 sp = sim.map.playerSpawn;
        foreach (double d in new[] { 6.0, 5.0, 7.0, 8.0 })
            for (int k = 0; k < 6; k++)
            {
                Husk h = sim.husks.Spawn("husk", new Vec3(sp.x + ux * d, sp.y + 0.25 * k, sp.z + uz * d), sp);
                if (h == null) continue;
                h.hp = h.maxHp = hp;
                return h;
            }
        return null;
    }

    static void Rea34Tick(SurvivalSim sim, Husk h, Vec3 pin, ref double yaw, CombatInput c)
    {
        if (h != null && h.alive) { h.position = pin; h.velocity = default; }
        double py = 0;
        if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
        Tick27(sim, yaw, py, c);
    }

    /// <summary>Swap to `gun` (asked on the first step) and wait until it is in hand and ready (at most 1 s).</summary>
    static void Rea34Swap(SurvivalSim sim, Husk h, Vec3 pin, ref double yaw, string gun, bool fireFromRaise, int fireAfterRaise)
    {
        int slot = sim.loadout.owned.IndexOf(gun), raised = -1;
        for (int s = 0; s < 240; s++)
        {
            bool inHand = sim.loadout.Weapon == gun;
            if (inHand && raised < 0) raised = s;
            bool fire = inHand && (fireFromRaise ? true : s - raised >= fireAfterRaise);
            Rea34Tick(sim, h, pin, ref yaw, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = fire, pressed = fire } });
            if (fire && (!fireFromRaise || s - raised > 60)) break;
        }
    }

    static void Rea34Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        Combo34Rules rules = Combo34Rules.From(cfg);
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Console.WriteLine($"rea34: arsenal {rules.arsenal} ({rules.arsenalStyle}), quickdraw {rules.quickdraw} ({rules.quickdrawS} s, {rules.quickdrawStyle}), whipReady {rules.whipReady} ({rules.whipReadyS} s); BEFORE = combo34 off, AFTER = on");

        // arsenal34
        var ars = new Dictionary<bool, (int ev, double style, int kills)>();
        foreach (bool on in new[] { false, true })
        {
            SurvivalSim sim = Rea34Sim(cfg, on, 3440, out double yaw, out double ux, out double uz);
            Husk h = Rea34Husk(sim, ux, uz, 40);
            if (h == null) { fails.Add("arsenal34: no husk"); break; }
            Vec3 pin = h.position;
            int ev = 0, kills = 0;
            sim.events.On("arsenal34", _ => ev++);
            sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; });
            double style0 = sim.style.core.total;
            Rea34Swap(sim, h, pin, ref yaw, "revolver", false, 40); // one KESTREL shot 40 steps after it is in hand (no quickdraw)
            for (int s = 0; s < 30; s++) Rea34Tick(sim, h, pin, ref yaw, new CombatInput { weapon = -1 });
            Rea34Swap(sim, h, pin, ref yaw, "shotgun", false, 40);
            for (int s = 0; s < 30; s++) Rea34Tick(sim, h, pin, ref yaw, new CombatInput { weapon = -1 });
            ars[on] = (ev, sim.style.core.total - style0, kills);
        }
        if (ars.Count == 2)
        {
            Console.WriteLine($"  arsenal34: BEFORE events {ars[false].ev}, kills {ars[false].kills}, style +{ars[false].style:0}; AFTER events {ars[true].ev}, kills {ars[true].kills}, style +{ars[true].style:0} (+{ars[true].style - ars[false].style:0}, want ~{rules.arsenalStyle})");
            Need(ars[false].ev == 0 && ars[true].ev == 1 && ars[true].kills == 1 && ars[true].style > ars[false].style, "arsenal34: one ARSENAL after, none before");
        }

        // quickdraw34
        var qd = new Dictionary<bool, int>();
        foreach (bool on in new[] { false, true })
        {
            SurvivalSim sim = Rea34Sim(cfg, on, 3441, out double yaw, out double ux, out double uz);
            Husk h = Rea34Husk(sim, ux, uz, 1e9);
            if (h == null) { fails.Add("quickdraw34: no husk"); break; }
            Vec3 pin = h.position;
            int ev = 0;
            sim.events.On("quickdraw34", _ => ev++);
            string first = sim.loadout.Weapon == "shotgun" ? "revolver" : "shotgun", second = first == "shotgun" ? "revolver" : "shotgun";
            Rea34Swap(sim, h, pin, ref yaw, first, true, 0); // the trigger held from the raise: a hit inside the window
            for (int s = 0; s < 60; s++) Rea34Tick(sim, h, pin, ref yaw, new CombatInput { weapon = -1 });
            int early = ev;
            Rea34Swap(sim, h, pin, ref yaw, second, false, Steps(0.5)); // the trigger 0.5 s after the raise: no quickdraw
            for (int s = 0; s < 60; s++) Rea34Tick(sim, h, pin, ref yaw, new CombatInput { weapon = -1 });
            qd[on] = early * 10 + (ev - early);
        }
        if (qd.Count == 2)
        {
            Console.WriteLine($"  quickdraw34: BEFORE {qd[false] / 10} (hit from the raise) / {qd[false] % 10} (0.5 s late); AFTER {qd[true] / 10} / {qd[true] % 10} (want 1 / 0)");
            Need(qd[false] == 0 && qd[true] / 10 == 1 && qd[true] % 10 == 0, "quickdraw34: one QUICKDRAW for the hit from the raise, none 0.5 s late, none before");
        }

        // whipready34
        foreach (bool on in new[] { false, true })
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n33Override = new N33Blocks { whip33 = true };
            sim.n34Override = new N34Blocks { bend34 = true, combo34 = on };
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            var si = new ScriptedInput();
            sim.UseInput(si);
            sim.StartRun(3442, null, null);
            sim.holdRunClock = true;
            if (sim.arcline21 == null) { fails.Add("whipready34: no ARCLINE"); break; }
            sim.arcline21.Own(true);
            Vec3 sp = sim.map.playerSpawn;
            sim.Respawn(sp, 0);
            (double yaw, double ux, double uz) = ArclineOpenDir(sim, sp, 15);
            for (int s = 0; s < 30; s++) G32Tick(sim, si, null, false, false, false);
            Husk c = null;
            for (int k = 0; k < 8 && c == null; k++) c = sim.husks.Spawn("charger", new Vec3(sp.x + ux * 10, sp.y + 0.25 * k, sp.z + uz * 10), sp);
            if (c == null) { fails.Add("whipready34: no charger"); break; }
            c.hp = c.maxHp = 1e9;
            Vec3 pin = c.position;
            int latchStep = -1, punchesAfter = 0;
            sim.events.On<Whip33Event>("whip33Latch", _ => latchStep = 0);
            int left = -1, readyAfter = -1, full = 0;
            for (int s = 0; s < Steps(2.0); s++)
            {
                if (c.alive) { c.position = pin; c.velocity = default; }
                (double yy, double pp) = ArclineLook(sim.Eye, c.AimPoint);
                si.SetLook(yy, pp);
                bool punch = s == 0 || (latchStep >= 0 && sim.loadout.PunchCooldownSteps == 0);
                bool grapple = s >= 1 && s < 3;
                si.Set(new InputFrame { grapple = new Button { held = grapple, pressed = s == 1 } }, new CombatInput { weapon = -1, punch = new Button { held = punch, pressed = punch } });
                sim.health.hp = sim.health.max;
                sim.mover.invulnerable = true;
                sim.hitstopLeft = 0;
                int before = sim.loadout.PunchCooldownSteps;
                sim.Step();
                if (s == 0) full = sim.loadout.PunchCooldownSteps;
                if (latchStep == 0 && left < 0) { left = sim.loadout.PunchCooldownSteps; latchStep = s; }
                if (latchStep > 0 && readyAfter < 0 && sim.loadout.PunchCooldownSteps == 0) readyAfter = s - latchStep;
                if (latchStep > 0 && sim.loadout.PunchCooldownSteps > before) punchesAfter++;
                _ = yaw;
            }
            Console.WriteLine($"  whipready34 {(on ? "AFTER " : "BEFORE")}: the punch's cooldown {full} steps; at the whip latch (step {latchStep}) {left} steps left; a punch ready {readyAfter} steps after the latch ({(readyAfter >= 0 ? (readyAfter * dt).ToString("0.000") : "-")} s); punches thrown after the latch {punchesAfter}; whip-readied {sim.whipReady34Count}");
            if (latchStep < 0) fails.Add($"whipready34 {(on ? "after" : "before")}: no whip latch");
            else if (on) Need(left <= Steps(rules.whipReadyS) && readyAfter <= Steps(rules.whipReadyS) && sim.whipReady34Count == 1, "whipready34: the punch ready within whipReadyS of the latch");
            else Need(sim.whipReady34Count == 0 && left > Steps(rules.whipReadyS), "whipready34 before: the punch's own cooldown (longer than whipReadyS) at the latch");
        }

        Console.WriteLine(fails.Count == 0 ? "rea34 GREEN" : $"rea34 FAIL: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
