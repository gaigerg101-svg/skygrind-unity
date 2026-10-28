// NIGHT27 (builder W): the weapon rows' headless judges. Reached through `weapons` (Harness.N26.Weapons.cs WeaponsCommand), no Program.cs line needed:
//   weapons --pulse27 [--off]        contract pulse27: the click swings every 300 steps (2.5 s) and hits heavy; KILLS fill five pips, hits do not (cap one
//                                    glove); LT is gated by a full glove (denied with why: pips / cooldown) and blasts with a 0.4 s boost; the blast's cooldown
//                                    is 15 s or longer. `--off` = design.n27.pulse27 off = NIGHT26's pulseLT: the RED side.
//   weapons --charge27 [--off]       contract chargeShots27: a tap fires at the press (no latency); a hold of 0.4 s + 0.6 s (pistol) / 0.8 s (shotgun) fires
//                                    the charged shot on release (pistol 4x, through 3 bodies; shotgun 3x with a knock); the cooldown is 10 s / 14 s; the
//                                    normal shot is never blocked; an early release spends nothing. `--off`: the normal shot only (the RED side).
//   weapons --n27                    the weapon-by-enemy table with both rows on (the re-fit: no gun best on every row).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>`weapons --n27`: the NIGHT27 rows the table's sims run with (null: the config's, as before).</summary>
    static N27Blocks? Weapons26N27;

    static SurvivalSim Sim27(GameConfig cfg, SurvivalMap map, bool pulse, bool charge, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { pulse27 = pulse, chargeShots27 = charge };
        if (!charge) sim.n31Override = new N31Blocks { ltAlways = N31Blocks.ForceOff ? false : N31Blocks.From(cfg).ltAlways }; // NIGHT31 W: a run without the charge has no n31 charge either (the other n31 rows as the config)
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    /// <summary>One step with the player topped up (the harness has no god mode: a bag husk in reach would end a long run).</summary>
    static void Tick27(SurvivalSim sim, double yaw, double pitch, CombatInput combat)
    {
        sim.health.hp = sim.health.max;
        Tick25(sim, yaw, pitch, combat);
    }

    /// <summary>A husk `d` m ahead along `u`, pinned there, with `hp` hit points; spawned again when it has died.</summary>
    static Husk Pin27(SurvivalSim sim, Husk h, Vec3 p, double ux, double uz, double d, double hp)
    {
        if (h == null || !h.alive)
        {
            h = sim.husks.Spawn("husk", new Vec3(p.x + ux * d, p.y, p.z + uz * d), p);
            if (h != null) h.hp = h.maxHp = hp;
        }
        if (h != null && h.alive)
        {
            h.position = new Vec3(p.x + ux * d, p.y, p.z + uz * d);
            h.hp = h.maxHp = hp;
        }
        return h;
    }

    // ---- pulse27 ----------------------------------------------------------------------------------------------------------------------------

    static void Pulse27Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        SurvivalSim sim = Sim27(cfg, map, !off, false, 2701);
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        PulseCharge27 pc = sim.pulse27;
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }

        int s = 0;
        var swingAt = new List<int>();
        var swingDmg = new List<double>();
        int kills = 0, hitsOnBag = 0;
        sim.events.On<MeleeEvent>("meleeSwing", e => swingAt.Add(s));
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") { swingDmg.Add(e.damage); if (!e.killed) hitsOnBag++; } });
        sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; });

        // A. the swing's cadence and weight: fire held for 8 s against a bag (hp huge: nothing dies, so no pip can fill by it)
        Husk h = null;
        int pipsAfterHits = -1;
        for (s = 0; s < (int)(8 / sim.step); s++)
        {
            h = Pin27(sim, h, sp, ux, uz, 1.6, 1e9);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = s == 0 } });
        }
        pipsAfterHits = pc != null ? pc.pips : -1;
        int swings = swingAt.Count;
        int gap = swings >= 2 ? swingAt[1] - swingAt[0] : -1;
        double dmg = swingDmg.Count > 0 ? swingDmg[0] : 0;
        double dps = swingDmg.Count > 0 && swings >= 2 ? swingDmg.Count * dmg / (8.0) : 0;
        int wantGap = (int)Math.Round(2.5 / sim.step);
        if (!off) Need(gap == wantGap, $"swing gap {gap} steps, want {wantGap}");
        int killsInA = kills;
        if (!off) Need(pipsAfterHits == killsInA && hitsOnBag >= 3, $"hits filled pips ({pipsAfterHits} pips, {killsInA} kills, {hitsOnBag} non-killing hits)");
        Console.WriteLine($"pulse27 A swing: {swings} swings in 8 s, gap {gap} steps ({gap * sim.step:0.00} s), {dmg:0.0} dmg a hit (gauntlet {cfg.Num("weapons.gauntlet.damage"):0}), {dmg * swings / 8.0:0.0} dps; {hitsOnBag} hits on a bag: pips {(pc != null ? pc.pips.ToString() : "n/a (no glove)")}");

        // B. kills fill the pips: one 1-hp husk a swing, one pip a kill, five fill the glove (the cap holds at 5 on a sixth)
        Console.WriteLine($"pulse27 A: {kills} kill(s) in the 8 s of swings, glove {pipsAfterHits} pip(s): the non-killing hits added none");
        int swing0 = swingAt.Count;
        var pipTrail = new List<int>();
        int killsBefore = kills;
        for (int k = 0; k < 6 && !off; k++)
        {
            int target = swingAt.Count + 1;
            for (int t = 0; t < (int)(3.0 / sim.step) && swingAt.Count < target; t++)
            {
                h = Pin27(sim, h, sp, ux, uz, 1.6, 1);
                double py = 0;
                if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
                Tick27(sim, yaw, py, new CombatInput { weapon = -1, fire = new Button { held = true, pressed = false } });
                s++;
            }
            Tick27(sim, yaw, 0, new CombatInput { weapon = -1 });
            s++;
            pipTrail.Add(pc.pips);
            if (Flag(args, "--dbg")) Console.WriteLine($"  dbg B k={k} swings {swingAt.Count} h alive {(h != null && h.alive)} hp {(h != null ? h.hp : -1)} kills {kills} hitsOnBag {hitsOnBag} weapon {sim.loadout.Weapon} active {sim.Active} state {sim.run.state} hp {sim.health.hp} ready {sim.loadout.ready} step {s} cd {sim.loadout.CooldownFractionOf("gauntlet")}");
        }
        var wantTrail = new List<int>();
        for (int k = 1; k <= 6; k++) wantTrail.Add(Math.Min(5, pipsAfterHits + k));
        if (!off) Need(string.Join(",", pipTrail) == string.Join(",", wantTrail), $"pip trail {string.Join(",", pipTrail)} want {string.Join(",", wantTrail)}");
        Console.WriteLine($"pulse27 B kills: {kills - killsBefore} kills by the gauntlet; pips after each kill {(off ? "n/a (no glove)" : string.Join(",", pipTrail))} (cap 5)");

        // C. LT: gated. Spend the glove first to see the gate with 4 pips: empty it by a blast, refill four, press LT: denied "pips".
        if (!off)
        {
            // glove is full now (5): LT blasts. Put two husks in the blast's reach first.
            // NIGHT31 W: B ends one step after the fifth kill's swing, inside that swing's ACTION (NIGHT30 rules: design.n30.rules.actionSeconds 0.35 s): a press
            // then is DROPPED, never buffered (PulseCharge27 StepPulse27: rules30.gunAction > 0), which is the game's rule; since NIGHT30 this C press fell in it and
            // the blast came at D's first press instead. Wait the action out (idle steps) before pressing, as a player's press after the swing lands.
            if (sim.n30.rules)
            {
                int wait = MovementMath.StepsFor(cfg.Num("design.n30.rules.actionSeconds"), sim.step) + 6;
                for (int k = 0; k < wait; k++) { Tick27(sim, yaw, 0, new CombatInput { weapon = -1 }); s++; }
            }
            Husk b1 = Pin27(sim, null, sp, ux, uz, 3.0, 1e9);
            Vec3 v0 = sim.mover.velocity;
            double speed0 = Math.Sqrt(v0.x * v0.x + v0.z * v0.z);
            double peak = speed0;
            int blastHitsBefore = 0;
            int fullAtPress = pc.pips, blastsBefore = pc.blasts;
            for (int t = 0; t < 60; t++)
            {
                b1 = Pin27(sim, b1, sp, ux, uz, 3.0, 1e9);
                double py = 0;
                if (b1 != null && b1.alive) (yaw, py) = ArclineLook(sim.Eye, b1.AimPoint);
                Tick27(sim, yaw, py, new CombatInput { weapon = -1, altFire = new Button { held = t < 20, pressed = t == 0 } });
                s++;
                Vec3 v = sim.mover.velocity;
                peak = Math.Max(peak, Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
                if (t == 0) blastHitsBefore = pc.blasts;
            }
            bool blasted = pc.blasts == blastsBefore + 1 && pc.pips == 0;
            int cdSteps = pc.cooldownSteps;
            Need(blasted, "LT with a full glove did not blast");
            int boostLen = (int)Math.Round(pc.p.boostSeconds / sim.step);
            Console.WriteLine($"pulse27 C blast: LT with {fullAtPress} pips -> blasts {pc.blasts}, pips {pc.pips}, peak speed {peak:0.0} m/s (was {speed0:0.0}), boost {boostLen} steps ({pc.p.boostSeconds:0.0} s), cooldown {cdSteps + 60} steps");

            // D. LT with fewer than 5 pips: denied "pips" (nothing spent), then 5 pips inside the cooldown: denied "cooldown", then after 15 s: blasts
            int denied0 = pc.denied;
            LtAlways31 lt31 = sim.loadout.lt31 != null && sim.loadout.lt31.active ? sim.loadout.lt31 : null; // NIGHT31 R6 (below)
            int ltAnswers0 = lt31 != null ? lt31.parriesOpened + lt31.denies : 0;
            Tick27(sim, yaw, 0, new CombatInput { weapon = -1 });
            s++;
            Tick27(sim, yaw, 0, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
            s++;
            string whyPips = pc.deniedWhy;
            // NIGHT31 R6 (design.n31.ltAlways on): no BLAST yet = the PARRY (or COOLING), not a pulse27 deny: the gate is the same, the answer is the parry's
            bool LtAnswered() => lt31 != null && lt31.parriesOpened + lt31.denies > ltAnswers0;
            if (lt31 != null) { whyPips = LtAnswered() ? "pips (ltAlways: " + lt31.word + ")" : "none"; Need(LtAnswered() && pc.blasts == 1, $"LT on an empty glove (ltAlways): parry / reason {LtAnswered()}"); }
            else Need(pc.denied == denied0 + 1 && whyPips == "pips" && pc.blasts == 1, $"LT on an empty glove: denied {pc.denied - denied0}, why {whyPips}");
            Tick27(sim, yaw, 0, new CombatInput { weapon = -1 });
            for (int k = 0; k < 5; k++) pc.AddKill(); // five kills (the entry the sentries call)
            if (lt31 != null) ltAnswers0 = lt31.parriesOpened + lt31.denies; // NIGHT31: a fresh baseline for the second press
            Tick27(sim, yaw, 0, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
            s++;
            string whyCd = pc.deniedWhy;
            if (lt31 != null) { whyCd = "cooldown (ltAlways: " + lt31.word + ")"; Need(pc.blasts == 1 && LtAnswered(), $"LT on a full glove inside the cooldown (ltAlways): blasts {pc.blasts}"); }
            else Need(pc.blasts == 1 && whyCd == "cooldown", $"LT on a full glove inside the cooldown: blasts {pc.blasts}, why {whyCd}");
            int cooldownLeft = pc.cooldownSteps;
            int waited = 0;
            while (pc.cooldownSteps > 0 && waited < 3000)
            {
                Tick27(sim, yaw, 0, new CombatInput { weapon = -1 });
                waited++;
                s++;
            }
            Tick27(sim, yaw, 0, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
            s++;
            double cdSeconds = (cooldownLeft + 60 + 2 + 2) * sim.step; // steps from the first blast to the second blast's press
            Need(pc.blasts == 2 && pc.p.blastCooldownSeconds >= 15.0, $"blasts {pc.blasts} after the cooldown ran out");
            Console.WriteLine($"pulse27 D gate: LT on {0} pips: {whyPips} (blasts stay 1); 5 pips inside the cooldown: {whyCd} ({cooldownLeft} steps left); after the cooldown ({pc.p.blastCooldownSeconds:0.0} s = {pc.BlastCooldownSteps} steps): blasts {pc.blasts}");
        }
        else
        {
            // RED: with the row off LT held with the gauntlet SWINGS (NIGHT26's pulseLT) and nothing is gated by pips or a cooldown.
            int sw0 = swingAt.Count;
            Husk b1 = null;
            for (int t = 0; t < 200; t++)
            {
                b1 = Pin27(sim, b1, sp, ux, uz, 1.6, 1e9);
                double py = 0;
                if (b1 != null && b1.alive) (yaw, py) = ArclineLook(sim.Eye, b1.AimPoint);
                Tick27(sim, yaw, py, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = t == 0 } });
                s++;
            }
            Console.WriteLine($"pulse27 RED (design.n27.pulse27 off = NIGHT26's pulseLT): swing gap {gap} steps (want {wantGap}), {dmg:0.0} dmg a hit (want {cfg.Num("weapons.gauntlet.damage") * cfg.Num("design.n27.pulse27.damageMult"):0}), no glove (pips n/a), LT held swings the gauntlet {swingAt.Count - sw0} times in 200 steps (no pips, no gate, no cooldown)");
            code = (gap == wantGap) ? 0 : 1; // the RED side must NOT meet the row
            Console.WriteLine($"pulse27 {(code == 1 ? "RED as expected" : "unexpected")}: the row off leaves NIGHT26's swing ({gap} steps) and ungated LT");
            code = code == 1 ? 0 : 1;
            return;
        }
        Console.WriteLine($"pulse27 {(fails.Count == 0 ? "PASS" : "FAIL")}{(fails.Count > 0 ? ": " + string.Join("; ", fails) : "")}");
        code = fails.Count == 0 ? 0 : 1;
    }

    /// <summary>`weapons --buy27`: the smoke's ARMORY step in the sim: stand on the spot, hold E (interact), with each NIGHT27 weapon row on or off: is the gate open after 10 s?</summary>
    static void Buy27Command(ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        bool allOk = true;
        foreach (var (pulse, charge) in new[] { (false, false), (true, false), (false, true), (true, true) })
        {
            SurvivalSim sim = Sim27(cfg, map, pulse, charge, 424242);
            BuySpot spot = sim.zones.spots.Find(sp => sp.zoneId == "armory");
            sim.economy.total = Math.Max(sim.economy.total, spot.price + 100);
            double before = sim.economy.total;
            sim.Respawn(spot.spot.pos, -90);
            for (int s = 0; s < (int)(10 / sim.step) && !sim.zones.IsOpen("armory"); s++)
                Tick27(sim, sim.input.Yaw, sim.input.Pitch, new CombatInput { weapon = -1, interact = new Button { held = true } });
            bool open = sim.zones.IsOpen("armory");
            Console.WriteLine($"buy27 pulse27 {(pulse ? "on" : "off")} chargeShots27 {(charge ? "on" : "off")}: ARMORY open {open}, points {before:0} -> {sim.economy.total:0}");
            allOk &= open;
        }
        code = allOk ? 0 : 1;
    }

    // ---- chargeShots27 ----------------------------------------------------------------------------------------------------------------------

    /// <summary>One scripted fire-button run: `held(s)` per step; returns the steps of each "fire" event and the hit damages (with the step of the hit).</summary>
    static (List<int> fires, List<(int step, double dmg)> hits, ChargeShots27 c) Charge27Run(GameConfig cfg, SurvivalMap map, string gun, bool on, Func<int, bool> held, int steps, int husks)
    {
        SurvivalSim sim = Sim27(cfg, map, false, on, 2702);
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        int s = 0;
        var fires = new List<int>();
        var hits = new List<(int, double)>();
        sim.events.On<string>("fire", w => { if (w == gun) fires.Add(s); });
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == gun) hits.Add((s, e.damage)); });
        var bodies = new Husk[husks];
        double[] dist = { 2.5, 4.0, 5.5, 7.0 };
        for (s = 0; s < steps; s++)
        {
            double py = 0;
            for (int k = 0; k < husks; k++) bodies[k] = Pin27(sim, bodies[k], sp, ux, uz, dist[k], 1e9);
            if (bodies[0] != null && bodies[0].alive) (yaw, py) = ArclineLook(sim.Eye, bodies[0].AimPoint);
            bool down = held(s);
            bool prev = s > 0 && held(s - 1);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && !prev } });
        }
        return (fires, hits, sim.loadout.charge27 ?? sim.charge27); // NIGHT31 W: the active charge (n31's replaces n27's on the loadout)
    }

    /// <summary>NIGHT31 W: the config block of the charge a charge-on run uses: design.n31.charge. while that row is on (its object replaces n27's on the
    /// loadout), else design.n27.chargeShots27.</summary>
    static string ActiveChargePrefix31(GameConfig cfg, SurvivalMap map)
    {
        SurvivalSim probe = Sim27(cfg, map, false, true, 2702);
        return probe.charge31 != null && ReferenceEquals(probe.loadout.charge27, probe.charge31) ? "design.n31.charge." : "design.n27.chargeShots27.";
    }

    static void Charge27Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => (int)Math.Round(sec / dt);
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        string P = ActiveChargePrefix31(cfg, map); // NIGHT31 W: the active charge's numbers
        Console.WriteLine($"charge27: the active charge's numbers: {P}");
        foreach (string gun in new[] { "revolver", "shotgun" })
        {
            int gi = gun == "revolver" ? 0 : 1;
            double holdS = cfg.Num(P + "holdSeconds"), fullS = cfg.Num(P + gun + ".fullSeconds"), cdS = cfg.Num(P + gun + ".cooldownSeconds");
            int husks = gun == "revolver" ? 4 : 1;
            int lead = Steps(0.5); // a settle before the first press
            // 1. a tap: pressed at `lead`, released the next step
            var tap = Charge27Run(cfg, map, gun, !off, s => s == lead, lead + Steps(2), husks);
            int tapLatency = tap.fires.Count > 0 ? tap.fires[0] - lead : -1;
            // normal damage per body of the revolver (first husk) / the shotgun's whole volley, from the tap
            // the normal volley / first-body hit: the mean of eight taps three seconds apart (the pellets' cone is random)
            int tapEvery = Steps(3.0);
            var taps = Charge27Run(cfg, map, gun, !off, s => s % tapEvery == lead, lead + tapEvery * 8, husks);
            double normalDmg = 0;
            int volleys = 0;
            for (int t = 0; t < 8; t++)
            {
                double sum = 0, first = 0;
                foreach (var h in taps.hits) if (h.step >= lead + t * tapEvery && h.step <= lead + t * tapEvery + 2) { sum += h.dmg; if (first == 0) first = h.dmg; }
                if (sum > 0) { normalDmg += gun == "revolver" ? first : sum; volleys++; }
            }
            normalDmg = volleys > 0 ? normalDmg / volleys : 0;
            // 2. a full hold: held from `lead` for holdS + fullS + 0.15 s, then released; observe 12 s after
            int holdEnd = lead + Steps(holdS + fullS) + Steps(0.15);
            var full = Charge27Run(cfg, map, gun, !off, s => s >= lead && s < holdEnd, holdEnd + Steps(1.0), husks);
            int chargedFires = full.c != null ? full.c.fired[gi] : 0;
            double chargedDmg = 0;
            int chargedBodies = 0;
            var seen = new HashSet<int>();
            foreach (var h in full.hits) if (h.step >= holdEnd - 1) { chargedDmg += h.dmg; chargedBodies++; }
            // 3. an early release: held holdS + 0.3 s only
            int earlyEnd = lead + Steps(holdS + 0.3);
            var early = Charge27Run(cfg, map, gun, !off, s => s >= lead && s < earlyEnd, earlyEnd + Steps(1.0), husks);
            int earlyFired = early.c != null ? early.c.fired[gi] : 0, earlyCancelled = early.c != null ? early.c.cancelled : 0;
            // 4. cooldown: two full holds one second apart: the second charge cannot start; held through the cooldown the normal shot still fires; after cdS a new charge fires
            int second = holdEnd + Steps(1.0), secondEnd = second + Steps(holdS + fullS + 0.15);
            int third = holdEnd + Steps(cdS) + Steps(0.5), thirdEnd = third + Steps(holdS + fullS + 0.15);
            var cool = Charge27Run(cfg, map, gun, !off, s => (s >= lead && s < holdEnd) || (s >= second && s < secondEnd) || (s >= third && s < thirdEnd), thirdEnd + Steps(1.0), husks);
            int firedTotal = cool.c != null ? cool.c.fired[gi] : 0;
            int normalDuringCooldown = 0;
            foreach (int f in cool.fires) if (f >= second && f < secondEnd) normalDuringCooldown++;
            // the same two minus the third: the second one must not have charged
            var cool2 = Charge27Run(cfg, map, gun, !off, s => (s >= lead && s < holdEnd) || (s >= second && s < secondEnd), secondEnd + Steps(1.0), husks);
            int firedTwo = cool2.c != null ? cool2.c.fired[gi] : 0;
            double ratio = normalDmg > 0 ? chargedDmg / normalDmg : 0;
            if (off)
            {
                Console.WriteLine($"charge27 RED {gun} (design.n27.chargeShots27 off): tap latency {tapLatency} steps; a {holdS + fullS + 0.15:0.00} s hold fires {full.fires.Count} normal shot(s) and NO charged shot ({chargedFires}); two holds a second apart: {firedTwo} charged");
                Need(chargedFires == 0 && firedTwo == 0, "a charged shot fired with the row off");
            }
            else
            {
                double want = gun == "revolver" ? cfg.Num(P + "revolver.damageMult") : cfg.Num(P + "shotgun.damageMult");
                int pierce = cfg.Int(P + "revolver.pierce");
                Console.WriteLine($"charge27 {gun}: tap fires {tapLatency} steps after the press (a normal shot {normalDmg:0.0}); hold {holdS:0.0} s + {fullS:0.0} s then release: {chargedFires} charged shot, {chargedBodies} body hit(s) for {chargedDmg:0.0} = {(gun == "revolver" ? chargedDmg / Math.Max(1, chargedBodies) / Math.Max(1e-9, normalDmg) : ratio):0.00}x a normal {(gun == "revolver" ? "hit per body" : "volley")} (mean of {volleys} taps) (want {want:0.#}x{(gun == "revolver" ? $", through {pierce}" : "")}); early release (0.3 s of charge): fired {earlyFired}, cancelled {earlyCancelled}; second hold 1 s later: {firedTwo} charged in total (cooldown {cdS:0} s = {Steps(cdS)} steps), {normalDuringCooldown} normal shot(s) fired meanwhile; after the cooldown: {firedTotal} charged in total");
                Need(tapLatency == 0, $"{gun} tap latency {tapLatency}");
                Need(chargedFires == 1, $"{gun} charged fires {chargedFires}");
                Need(earlyFired == 0 && earlyCancelled == 1, $"{gun} early release fired {earlyFired} cancelled {earlyCancelled}");
                Need(firedTwo == 1 && firedTotal == 2, $"{gun} cooldown: {firedTwo} / {firedTotal}");
                Need(normalDuringCooldown >= 1, $"{gun} normal fire blocked during the cooldown ({normalDuringCooldown})");
                if (gun == "revolver") Need(chargedBodies == pierce, $"revolver pierce {chargedBodies}");
                if (gun == "revolver") Need(Math.Abs(chargedDmg / Math.Max(1, chargedBodies) / normalDmg - want) < 0.25, $"revolver per-body {chargedDmg / Math.Max(1, chargedBodies) / normalDmg:0.00}x, want {want}");
                else Need(ratio > want * 0.75 && ratio < want * 1.35, $"shotgun ratio {ratio:0.00}x, want about {want}");
            }
        }
        Console.WriteLine($"charge27 {(off ? "RED side (row off): " : "")}{(fails.Count == 0 ? (off ? "no charged shot with the row off" : "PASS") : "FAIL: " + string.Join("; ", fails))}");
        code = off ? (fails.Count == 0 ? 0 : 1) : (fails.Count == 0 ? 0 : 1);
    }
}
