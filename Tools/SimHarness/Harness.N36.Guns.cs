// NIGHT36 builder F (gun feel): F's headless judges. ONE entry method, N36Guns(cmd, args, ref code) (declared and called in Program.cs).
//   n36tap [--off36]   design.n36.tap36 per gun (KESTREL, THUNDERCLAP), scripted RT at a husk 4 m ahead (design.n31.charge + design.n35.charge35 on, the
//                      n27 row off, tap36 forced on unless --off36):
//                      CLICK 60 ms: ONE normal shot ON THE PRESS STEP (+0), no charge, no CHARGING;
//                      HOLD 0.8 s + release: a normal shot ON THE PRESS STEP, then ONE charged shot on the release step (both hit), charge35Start / Full
//                      once, charge35Start after holdStartS (not before), no other shots;
//                      SHORT HOLD 0.3 s (past holdStartS, short of FULL): the press shot only, the partial charge drains (no second shot);
//                      HELD 4 s at husks with real hit points: the press shot, the full charge fires itself (holdMaxS), the hold keeps killing.
//                      GREEN = every PASS line with tap36 on. `--off36`: charge35's rule (the tap fires on the RELEASE): the "on the press step" lines
//                      are RED (the RED side of L-014).
//   n36crunch [--off36]  crunch36's freeze tiers through Core's ImpactRules (the Game's freeze rules, headless): a KESTREL body hit (no kill), a
//                      punch hit, a STITCHER-like stream of 10 body hits a second for 3 s, and a kill right after a body hit; prints the freeze per tier
//                      and the body share of a rolling second. GREEN (on): a body hit freezes bodyHitS (15-20 ms), a punch hit punchHitS (0.1 s),
//                      the stream stays inside its share and the total budget, the kill after a body hit still freezes. --off36: a body hit has no
//                      freeze (RED side).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N36Guns(string cmd, string[] args, ref int code)
    {
        if (cmd == "n36tap") N36TapCommand(args, ref code);
        else if (cmd == "n36crunch") N36CrunchCommand(args, ref code);
    }

    sealed class Tap36Result
    {
        public int fires, charged, chargedHits, starts, fulls, hits, kills, killsLate, autoReleased, firstStart = -1, firstCharged = -1, missing;
        public readonly List<int> fireSteps = new List<int>();
    }

    static Tap36Result Tap36Run(GameConfig cfg, SurvivalMap map, string gun, bool on, Func<int, bool> held, int steps, bool realHp, int lateFrom)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { chargeShots27 = false };
        sim.n31Override = new N31Blocks { charge = true };
        sim.n35Override = new N35Blocks { charge35 = true };
        sim.n36Override = new N36Blocks { tap36 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3601, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        var r = new Tap36Result { missing = sim.tap36Missing };
        int s = 0;
        sim.events.On<string>("fire", w => { if (w != gun) return; r.fires++; r.fireSteps.Add(s); });
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { r.charged++; r.chargedHits += e.hits; if (r.firstCharged < 0) r.firstCharged = s; });
        sim.events.On<ChargeShot27Event>("charge35Start", e => { r.starts++; if (r.firstStart < 0) r.firstStart = s; });
        sim.events.On<ChargeShot27Event>("charge35Full", e => r.fulls++);
        sim.events.On<HitEvent>("hit", h => { if (h.owner == "player" && h.weapon == gun) r.hits++; });
        Husk body = null;
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
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && !prev } });
        }
        if (sim.loadout.charge27 != null) r.autoReleased = sim.loadout.charge27.autoReleased;
        return r;
    }

    static void N36TapCommand(string[] args, ref int code)
    {
        bool on = !N36Blocks.ForceOff;
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
        double holdStartS = cfg.Num("design.n36.tap36.holdStartS");
        Console.WriteLine($"n36tap: {(on ? "design.n36.tap36 ON (forced on here; the row's own enabled is " + cfg.Design("n36.tap36.enabled") + ")" : "--off36: charge35's rule (the RED side)")}; holdStartS {holdStartS} s ({Steps(holdStartS)} steps); design.n31.charge + n35.charge35 on");
        foreach (string gun in new[] { "revolver", "shotgun" })
        {
            string name = gun == "revolver" ? "KESTREL" : "THUNDERCLAP";
            double fullS = cfg.Num("design.n31.charge." + gun + ".fullSeconds");
            int lead = Steps(0.5);
            int click = Steps(0.06), hold = Steps(0.8), shortHold = Steps(0.3);
            var c = Tap36Run(cfg, map, gun, on, s => s >= lead && s < lead + click, lead + Steps(1), false, int.MaxValue);
            var h = Tap36Run(cfg, map, gun, on, s => s >= lead && s < lead + hold, lead + hold + Steps(0.3), false, int.MaxValue);
            var p = Tap36Run(cfg, map, gun, on, s => s >= lead && s < lead + shortHold, lead + shortHold + Steps(1), false, int.MaxValue);
            int late = lead + Steps(1.5);
            var held = Tap36Run(cfg, map, gun, on, s => s >= lead && s < lead + Steps(4), lead + Steps(4), true, late);
            string Rel(List<int> l) => l.Count == 0 ? "none" : string.Join(", ", l.ConvertAll(x => "+" + (x - lead)));
            Console.WriteLine($" {name} ({gun}): full {fullS} s from the press; press step = +0, release step = +hold steps{(c.missing > 0 ? "; tap36Missing " + c.missing : "")}");
            Console.WriteLine($"  CLICK {click} steps (60 ms): shots at [{Rel(c.fireSteps)}], charged {c.charged}, charge35Start {c.starts}, hits {c.hits}");
            Console.WriteLine($"  HOLD {hold} steps (0.8 s): shots at [{Rel(h.fireSteps)}], charged {h.charged} at +{(h.firstCharged >= 0 ? h.firstCharged - lead : -1)} ({h.chargedHits} hit), charge35Start at +{(h.firstStart >= 0 ? h.firstStart - lead : -1)} / Full {h.fulls}");
            Console.WriteLine($"  SHORT HOLD {shortHold} steps (0.3 s): shots at [{Rel(p.fireSteps)}], charged {p.charged}, charge35Start {p.starts}");
            Console.WriteLine($"  HELD 4 s (real husks): shots at [{Rel(held.fireSteps.GetRange(0, Math.Min(6, held.fireSteps.Count)))}{(held.fireSteps.Count > 6 ? ", ..." : "")}] ({held.fires}), kills {held.kills} ({held.killsLate} after 1.5 s), charged {held.charged} (fired itself {held.autoReleased})");
            Check(c.fireSteps.Count == 1 && c.fireSteps[0] == lead && c.charged == 0 && c.starts == 0, $"{name}: a 60 ms click fires ONE shot ON ITS PRESS STEP, no charge ([{Rel(c.fireSteps)}])");
            Check(h.fireSteps.Count >= 1 && h.fireSteps[0] == lead, $"{name}: a 0.8 s hold fires a normal shot ON THE PRESS STEP ([{Rel(h.fireSteps)}])");
            Check(h.charged == 1 && h.firstCharged == lead + hold && h.chargedHits >= 1, $"{name}: ... and ONE charged shot on the release step +{hold} (at +{(h.firstCharged >= 0 ? h.firstCharged - lead : -1)}, {h.chargedHits} hit)");
            Check(h.fires == 2, $"{name}: the 0.8 s hold is exactly two shots, the tap and the charge ({h.fires})");
            Check(h.starts == 1 && h.firstStart - lead >= Steps(holdStartS), $"{name}: CHARGING starts only past holdStartS (at +{(h.firstStart >= 0 ? h.firstStart - lead : -1)} >= {Steps(holdStartS)})");
            Check(p.fires == 1 && p.charged == 0, $"{name}: a partial charge drains: the press shot only ({p.fires} shots, {p.charged} charged)");
            Check(held.fireSteps.Count > 0 && held.fireSteps[0] == lead && held.charged >= 1 && held.autoReleased >= 1 && held.killsLate > 0, $"{name}: a held trigger fires at the press and never stalls (charged {held.charged}, fired itself {held.autoReleased}, kills after 1.5 s {held.killsLate})");
        }
        Console.WriteLine(fails.Count == 0 ? "n36tap GREEN" : $"n36tap RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    /// <summary>A 60 fps frame loop over Core's ImpactRules (the Game's ImpactFrames shape: BeginFrame, then the frame's sim steps, each EndStep):
    /// `hits(step)` delivers the step's hit events. Returns the rules after `seconds`.</summary>
    static ImpactRules Crunch36Loop(GameConfig cfg, bool on, double seconds, Action<ImpactRules, int> hits, Action<ImpactRules, int> perStep = null)
    {
        var r = new ImpactRules(cfg) { crunch36 = on };
        r.ApplySettings(true, null, true, false);
        double dt = cfg.Dt, frame = 1 / 60.0, acc = 0;
        int step = 0, total = MovementMath.StepsFor(seconds, dt);
        while (step < total)
        {
            r.ClearPending();
            double share = r.BeginFrame(frame, false);
            acc += frame * share;
            while (acc >= dt && step < total)
            {
                acc -= dt;
                hits(r, step);
                bool froze = r.EndStep(false);
                perStep?.Invoke(r, step);
                step++;
                if (froze) { acc = 0; break; }
            }
        }
        return r;
    }

    static void N36CrunchCommand(string[] args, ref int code)
    {
        bool on = !N36Blocks.ForceOff;
        GameConfig cfg = Config();
        var fails = new List<string>();
        void Check(bool ok, string what)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} {what}");
            if (!ok) fails.Add(what);
        }
        const string p = "design.n36.crunch36.";
        double bodyS = cfg.Num(p + "bodyHitS"), punchS = cfg.Num(p + "punchHitS"), share = cfg.Num(p + "bodyBudgetPerSecond"), perSecond = cfg.Num("design.impact.budget.perSecond");
        Console.WriteLine($"n36crunch: crunch36 {(on ? "ON (forced on here; the row's own enabled is " + cfg.Design("n36.crunch36.enabled") + ")" : "OFF (--off36: the RED side)")}; bodyHitS {bodyS}, punchHitS {punchS}, body share {share} s/s, total {perSecond} s/s");
        HitEvent Hit(string w, int shot, bool killed = false, bool melee = false) => new HitEvent { owner = "player", weapon = w, shot = shot, killed = killed, melee = melee, damage = 25, distance = 6 };
        // 1. one KESTREL body hit
        var a = Crunch36Loop(cfg, on, 0.5, (r, s) => { if (s == 10) r.OnHit(Hit("revolver", 1)); });
        double aS = a.frozenSeconds[(int)ImpactTier.BodyHit];
        Console.WriteLine($"  KESTREL body hit: bodyHit freezes {a.freezes[(int)ImpactTier.BodyHit]}, {aS * 1000:0.0} ms");
        // 2. one punch hit
        var b = Crunch36Loop(cfg, on, 0.5, (r, s) => { if (s == 10) r.OnHit(Hit("punch", 1, false, true)); });
        double bS = b.frozenSeconds[(int)ImpactTier.PunchHit];
        Console.WriteLine($"  punch hit: punchHit freezes {b.freezes[(int)ImpactTier.PunchHit]}, {bS * 1000:0.0} ms");
        // 3. a STITCHER-like stream: 10 body hits a second for 3 s (a new shot each), then the worst rolling second's frozen time
        double worstBody = 0, worstAll = 0;
        int every = MovementMath.StepsFor(0.1, cfg.Dt), shotN = 100;
        var c = Crunch36Loop(cfg, on, 3.0, (r, s) => { if (s % every == 0) r.OnHit(Hit("nailgun", ++shotN)); },
            (r, s) => { worstBody = Math.Max(worstBody, r.FrozenIn(r.Clock - 1, r.Clock, 2)); worstAll = Math.Max(worstAll, r.FrozenBetween(r.Clock - 1, r.Clock)); });
        int cN = c.freezes[(int)ImpactTier.BodyHit];
        Console.WriteLine($"  stream 10 hits/s x 3 s: {cN} body freezes ({c.skips[(int)ImpactTier.BodyHit]} skipped), {c.frozenSeconds[(int)ImpactTier.BodyHit] * 1000:0} ms; worst rolling second: body {worstBody * 1000:0} ms, all {worstAll * 1000:0} ms");
        // 4. a body hit, then a kill 0.05 s later
        int killAt = 10 + MovementMath.StepsFor(0.05, cfg.Dt);
        var d = Crunch36Loop(cfg, on, 0.6, (r, s) => { if (s == 10) r.OnHit(Hit("revolver", 1)); if (s == killAt) r.OnHit(Hit("revolver", 2, true)); });
        Console.WriteLine($"  body hit then a kill 0.05 s later: bodyHit {d.freezes[(int)ImpactTier.BodyHit]}, kill {d.freezes[(int)ImpactTier.Kill]} ({d.frozenSeconds[(int)ImpactTier.Kill] * 1000:0.0} ms), kill skips {d.skips[(int)ImpactTier.Kill]}");
        Check(a.freezes[(int)ImpactTier.BodyHit] == 1 && aS >= 0.015 - 1e-9 && aS <= 0.020 + 1e-9, $"a KESTREL body hit freezes 15-20 ms ({aS * 1000:0.0} ms)");
        Check(b.freezes[(int)ImpactTier.PunchHit] == 1 && Math.Abs(bS - 0.1) < 1e-9, $"a punch hit freezes 0.1 s ({bS * 1000:0.0} ms)");
        Check(cN >= 10 && worstBody <= share + 1e-9 && worstAll <= perSecond + 1e-9, $"the stream freezes often ({cN}) inside its share ({worstBody * 1000:0} <= {share * 1000:0} ms) and the total ({worstAll * 1000:0} <= {perSecond * 1000:0} ms)");
        Check(d.freezes[(int)ImpactTier.Kill] == 1, $"a kill right after a body hit still freezes ({d.freezes[(int)ImpactTier.Kill]})");
        Console.WriteLine(fails.Count == 0 ? "n36crunch GREEN" : $"n36crunch RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

}
