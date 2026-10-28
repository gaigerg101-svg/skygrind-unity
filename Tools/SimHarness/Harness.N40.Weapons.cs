// NIGHT40 builder W1 (the weapons): W1's headless judges. ONE entry method, N40Weapons(cmd, args, ref code) (declared and called in Program.cs).
//   n40weapons [--off40]  design.n40.charge40 (forced on unless --off40; the game's charge rows on: n31.charge, n35.charge35, n36.tap36, n39.p39; the
//                         n27 row off), a husk 4 m ahead, scripted RT. Press step = +0.
//                         FULL: the KESTREL full at 0.9 s (108 steps), the THUNDERCLAP at 1.2 s (144) from the press (charge35Full's step);
//                         GROUND CLICKS 40-140 ms on the THUNDERCLAP: ONE volley ON THE RELEASE STEP, no charge (the added latency = the click);
//                         GROUND HOLD 1.4 s: NO volley while held, ONE slug on the release, CHARGING from shotgunTapS;
//                         GROUND SHORT HOLD 0.5 s: the volley on the release (partialFires), no slug;
//                         AIR: a 60 ms click fires ON its press step (0 ms added) and the boost acts on that step; a 1.4 s air hold keeps tap36
//                         (the press volley, then the slug on the release);
//                         PRESSED 3 STEPS (L-053: a Player press spans every sim step of its frame): one volley, on the ground and in the air;
//                         CORRECTION: an 80 ms ground click while the look swings 3 deg a step: the volley leaves along the PRESS's aim;
//                         COOLING: a ground click inside the slug's cooldown fires ON its press step.
//                         GREEN = every PASS line with charge40 on. `--off40`: NIGHT39 (tap36 on the THUNDERCLAP, n31's fullSeconds): the
//                         ground-release and FULL lines go RED (L-014's RED side).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N40Weapons(string cmd, string[] args, ref int code)
    {
        if (cmd == "n40weapons" && args.Length > 1 && args[1] == "rhythm") N40RhythmCommand(ref code);
        else if (cmd == "n40weapons") N40WeaponsCommand(args, ref code);
    }

    /// <summary>
    /// n40weapons rhythm: every animation a KESTREL shot starts ends within UnityTuning.Pistol40Share of the time to its next shot (a tap: the
    /// interval, design.n39.p39's; the charged shot: design.n35.charge35.revolver.cooldownS), from the numbers the views read (an animation's end =
    /// its attack + settle; a spring's = its envelope at 5 %: ln 20 / (zeta 2 pi f)); NIGHT39's ends beside them (the RED side). Then pump40's ends.
    /// The Player's own proof is Pistol39View's frame log ("rhythm40 tap ..." lines in the Player log, charge40 on). GREEN = every NIGHT40 line inside.
    /// </summary>
    static void N40RhythmCommand(ref int code)
    {
        GameConfig cfg = Config();
        double dt = cfg.Dt;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        double interval = MovementMath.StepsFor(cfg.Num("weapons.revolver.fireInterval") * cfg.Num("design.n39.p39.intervalMult"), dt) * dt;
        double cool = cfg.Num("design.n35.charge35.revolver.cooldownS"), share = Skygrind.Game.UnityTuning.Pistol40Share;
        double capT = share * interval, capC = share * cool;
        // the NIGHT36 hammer's numbers live in UnityTuning.Guns36.cs (Unity types inside: read from the source)
        string g36 = System.IO.File.ReadAllText(System.IO.Path.Combine(RepoRoot,"Assets/Scripts/Game/UnityTuning.Guns36.cs"));
        var hm = System.Text.RegularExpressions.Regex.Match(g36, @"Hammer36HoldS = ([0-9.]+)f, Hammer36CockS = ([0-9.]+)f");
        double hammer = hm.Success ? double.Parse(hm.Groups[1].Value, ci) + double.Parse(hm.Groups[2].Value, ci) : -1;
        double Spring(double[] row) => Math.Log(20) / (row[1] * 2 * Math.PI * row[0]);
        var fails = new List<string>();
        string S(double s) => s.ToString("0.000", ci);
        void Line(string what, double n39, double n40, double cap)
        {
            bool ok = n40 >= 0 && n40 <= cap;
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} {what}: ends +{S(n40)} s ({(n40 / (cap / share) * 100).ToString("0", ci)} %; NIGHT39 +{S(n39)} s, {(n39 / (cap / share) * 100).ToString("0", ci)} %{(n39 > cap ? " LATE" : "")}); cap {S(cap)} s");
            if (!ok) fails.Add(what);
        }
        Console.WriteLine($"n40weapons rhythm: the KESTREL's tap interval {S(interval)} s (cap {share:0.00} x = {S(capT)} s), its charged shot's cooldown {S(cool)} s (cap {S(capC)} s)");
        Line("TAP gun kick (Pistol39View; NIGHT40 ease-out)", Skygrind.Game.UnityTuning.Pistol39KickAttackS + Skygrind.Game.UnityTuning.Pistol39KickSettleS, Skygrind.Game.UnityTuning.Pistol40KickAttackS + Skygrind.Game.UnityTuning.Pistol40KickSettleS, capT);
        Line("TAP view kick", Skygrind.Game.UnityTuning.Pistol39CamAttackS + Skygrind.Game.UnityTuning.Pistol39CamSettleS, Skygrind.Game.UnityTuning.Pistol40CamAttackS + Skygrind.Game.UnityTuning.Pistol40CamSettleS, capT);
        double cock = Skygrind.Game.UnityTuning.Pistol39CockAttackS + Skygrind.Game.UnityTuning.Pistol39CockSettleS;
        Line("PRESS cock (p39Press; unchanged)", cock, cock, capT);
        Line("TAP hammer fall + re-cock (NIGHT36 crunch36; unchanged)", hammer, hammer, capT);
        double cyl = cfg.Num("viewmodel.cylinderTurnTime");
        Line("TAP cylinder step (the web's viewmodel.cylinderTurnTime; unchanged)", cyl, cyl, capT);
        double sa = Spring(Skygrind.Game.UnityTuning.HandsKickRevolverA), sb = Spring(Skygrind.Game.UnityTuning.HandsKickRevolverB);
        Line("TAP spring kick A (HandsKickRevolverA, 5 % envelope; unchanged)", sa, sa, capT);
        Line("TAP spring kick B (HandsKickRevolverB; unchanged)", sb, sb, capT);
        Line("TAP crosshair quarter turn + snap (H1 wires Pistol40TapSpinS + Pistol40TapSnapS)", interval + Skygrind.Game.UnityTuning.Hud38SnapTime, Skygrind.Game.UnityTuning.Pistol40TapSpinS + Skygrind.Game.UnityTuning.Pistol40TapSnapS, capT);
        double can = Skygrind.Game.UnityTuning.Pistol39CannonAttackS + Skygrind.Game.UnityTuning.Pistol39CannonSettleS;
        Line("CANNON gun kick (ease-out with charge40)", can, can, capC);
        double canCam = Skygrind.Game.UnityTuning.Pistol39CannonCamAttackS + Skygrind.Game.UnityTuning.Pistol39CannonCamSettleS;
        Line("CANNON view kick", canCam, canCam, capC);
        Line("CANNON crosshair full turn + snap (H1 wires Pistol40CannonSpinS + Pistol40CannonSnapS)", cool + Skygrind.Game.UnityTuning.Hud38SnapTime, Skygrind.Game.UnityTuning.Pistol40CannonSpinS + Skygrind.Game.UnityTuning.Pistol40CannonSnapS, capC);
        // pump40 (the THUNDERCLAP; the interval unchanged)
        double sgInt = cfg.Num("weapons.shotgun.fireInterval") * cfg.Num("design.n24.shotgunTune.intervalMult") * cfg.Num("design.n29.shotgun.fireIntervalMult");
        double p39 = cfg.Num("weapons.shotgun.pumpDelay") + cfg.Num("viewmodel.pumpDuration"), p40 = Skygrind.Game.UnityTuning.Pump40DelayS + Skygrind.Game.UnityTuning.Pump40DurationS;
        Console.WriteLine($"  pump40: the pump ends +{S(p40)} s ({(p40 / sgInt * 100).ToString("0", ci)} % of the THUNDERCLAP's {S(sgInt)} s interval; the web's +{S(p39)} s, {(p39 / sgInt * 100).ToString("0", ci)} %); the kick spring's 5 % envelope +{S(Spring(Skygrind.Game.UnityTuning.Pump40KickA))} s (was +{S(Spring(Skygrind.Game.UnityTuning.HandsKickShotgunA))} s at {Skygrind.Game.UnityTuning.HandsKickShotgunA[0]} Hz)");
        bool pumpOk = p40 < p39 && p40 <= share * sgInt && Spring(Skygrind.Game.UnityTuning.Pump40KickA) < Spring(Skygrind.Game.UnityTuning.HandsKickShotgunA);
        Console.WriteLine($"  {(pumpOk ? "PASS" : "RED ")} pump40: faster than the web's and inside {share:0.00} of the interval");
        if (!pumpOk) fails.Add("pump40");
        Console.WriteLine(fails.Count == 0 ? "n40weapons rhythm GREEN" : $"n40weapons rhythm RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    sealed class C40Result
    {
        public int fires, charged, starts, fullAt = -1, firstStart = -1, firstCharged = -1, hits, groundPresses, airPresses, missing, corrected, presses40;
        public readonly List<int> fireSteps = new List<int>();
        public readonly List<int> chargedSteps = new List<int>();
        public double firstShotOffPressDeg = -1, firstShotOffReleaseDeg = -1, corrW = -1, pressVy = double.NaN, preVy = double.NaN;
        public bool firstShotHit;
        /// <summary>Normal shots (fire events not on a charged shot's step).</summary>
        public List<int> Normal => fireSteps.FindAll(x => !chargedSteps.Contains(x));
    }

    /// <summary>One scripted run. `held(s)` the trigger; `pressed(s)` the press flag (null: the edge of held); `air(s)` pins the player 4 m up, still;
    /// `pitchDeg` the look's pitch (null: at the husk); `swingDeg(s)` added yaw.</summary>
    static C40Result C40Run(GameConfig cfg, SurvivalMap map, string gun, bool on, Func<int, bool> held, int steps, Func<int, bool> pressed = null,
        Func<int, bool> air = null, double? pitchDeg = null, Func<int, double> swingDeg = null)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { chargeShots27 = false };
        sim.n31Override = new N31Blocks { charge = true };
        sim.n35Override = new N35Blocks { charge35 = true };
        sim.n36Override = new N36Blocks { tap36 = true };
        sim.n39Override = new N39Blocks { p39 = true };
        sim.n40Override = on ? new N40Blocks { charge40 = true } : N40Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3601, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        var r = new C40Result { missing = sim.c40Missing };
        int s = 0;
        Vec3 pressDir = default, releaseDir = default;
        bool firstSeen = false;
        sim.events.On<string>("fire", w => { if (w != gun) return; r.fires++; r.fireSteps.Add(s); });
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (e.gun != gun) return; r.charged++; r.chargedSteps.Add(s); if (r.firstCharged < 0) r.firstCharged = s; });
        sim.events.On<ChargeShot27Event>("charge35Start", e => { if (e.gun == gun && r.firstStart < 0) r.firstStart = s; if (e.gun == gun) r.starts++; });
        sim.events.On<ChargeShot27Event>("charge35Full", e => { if (e.gun == gun && r.fullAt < 0) r.fullAt = s; });
        sim.events.On<ChargeShot27Event>("charge40Press", e => r.presses40++);
        sim.events.On<HitEvent>("hit", h => { if (h.owner == "player" && h.weapon == gun) r.hits++; });
        sim.events.On<ShotEvent>("shotResult", e =>
        {
            if (firstSeen || e.weapon != gun) return;
            firstSeen = true;
            r.firstShotHit = e.surface == "enemy";
            if (pressDir.Length > 0) r.firstShotOffPressDeg = AngleDeg(e.dir, pressDir);
            if (releaseDir.Length > 0) r.firstShotOffReleaseDeg = AngleDeg(e.dir, releaseDir);
            if (sim.charge35 != null) r.corrW = sim.charge35.lastCorrW39;
        });
        Husk body = null;
        Vec3 hang = new Vec3(sp.x, sp.y + 4, sp.z);
        bool pressSeen = false;
        for (s = 0; s < steps; s++)
        {
            bool up = air != null && air(s);
            if (up)
            {
                sim.mover.position = hang;
                sim.mover.velocity = new Vec3(0, 0, 0);
                sim.mover.grounded = false;
            }
            body = Pin27(sim, body, sp, ux, uz, 4.0, 1e9);
            double py = 0;
            if (body != null && body.alive) (yaw, py) = ArclineLook(sim.Eye, body.AimPoint);
            if (pitchDeg.HasValue) py = pitchDeg.Value * Math.PI / 180;
            double y = yaw + (swingDeg != null ? swingDeg(s) * Math.PI / 180 : 0);
            bool down = held(s), prev = s > 0 && held(s - 1);
            bool pr = pressed != null ? pressed(s) : down && !prev;
            double cp = Math.Cos(py);
            var dir = new Vec3(-Math.Sin(y) * cp, Math.Sin(py), -Math.Cos(y) * cp);
            if (down && !prev && pressDir.Length == 0) pressDir = dir;
            if (!down && prev && releaseDir.Length == 0) releaseDir = dir;
            bool first = down && !prev && !pressSeen;
            if (first) r.preVy = sim.mover.velocity.y;
            Tick27(sim, y, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = pr } });
            if (first) { r.pressVy = sim.mover.velocity.y; pressSeen = true; }
        }
        if (sim.charge35 != null)
        {
            r.groundPresses = sim.charge35.c40GroundPresses;
            r.airPresses = sim.charge35.c40AirPresses;
            r.corrected = sim.charge35.corrected39;
        }
        return r;
    }

    static void N40WeaponsCommand(string[] args, ref int code)
    {
        bool on = !N40Blocks.ForceOff;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        string Ms(int steps) => (steps * dt * 1000).ToString("0", ci);
        var fails = new List<string>();
        void Check(bool ok, string what)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} {what}");
            if (!ok) fails.Add(what);
        }
        const string p = "design.n40.charge40.";
        double revFull = cfg.Num(p + "revolverFullS"), sgFull = cfg.Num(p + "shotgunFullS"), sgTap = cfg.Num(p + "shotgunTapS");
        double n31Rev = cfg.Num("design.n31.charge.revolver.fullSeconds"), n31Sg = cfg.Num("design.n31.charge.shotgun.fullSeconds");
        Console.WriteLine($"n40weapons: {(on ? "design.n40.charge40 ON (forced on here; the row's own enabled is " + cfg.Design("n40.charge40.enabled") + ")" : "--off40: NIGHT39 (tap36 THUNDERCLAP, n31's FULL) — the RED side")}; FULL KESTREL {revFull} s / THUNDERCLAP {sgFull} s (n31: {n31Rev} / {n31Sg}), shotgunTapS {sgTap} s ({Steps(sgTap)} steps); n31.charge + n35.charge35 + n36.tap36 + n39.p39 on");
        int lead = Steps(0.5);
        string Rel(List<int> l) => l.Count == 0 ? "none" : string.Join(", ", l.ConvertAll(x => "+" + (x - lead)));
        Func<int, bool> always = s => true;

        // 1. FULL: the step charge35Full fires on a long hold (from the press)
        int longHold = Steps(1.6);
        var fk = C40Run(cfg, map, "revolver", on, s => s >= lead && s < lead + longHold, lead + longHold + Steps(0.3));
        var fs = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + longHold, lead + longHold + Steps(0.3));
        int wantK = Steps(on ? revFull : n31Rev), wantS = Steps(on ? sgFull : n31Sg);
        Console.WriteLine($"  FULL: KESTREL charge35Full at +{fk.fullAt - lead} ({Ms(fk.fullAt - lead)} ms), THUNDERCLAP at +{fs.fullAt - lead} ({Ms(fs.fullAt - lead)} ms); charged {fk.charged} / {fs.charged}{(fk.missing + fs.missing > 0 ? "; c40Missing " + (fk.missing + fs.missing) : "")}");
        // the press step is the hold's first step, so FULL lands on held step N = +(N-1) (NIGHT39's 0.5 s: 60 steps, +59)
        Check(fk.fullAt - lead + 1 == Steps(revFull) && fk.charged == 1, $"KESTREL: FULL on held step {fk.fullAt - lead + 1} ({Ms(fk.fullAt - lead + 1)} ms of hold; want {revFull * 1000:0} ms = {Steps(revFull)} steps; NIGHT39 {n31Rev * 1000:0})");
        Check(fs.fullAt - lead + 1 == Steps(sgFull) && fs.charged == 1, $"THUNDERCLAP: FULL on held step {fs.fullAt - lead + 1} ({Ms(fs.fullAt - lead + 1)} ms of hold; want {sgFull * 1000:0} ms = {Steps(sgFull)} steps; NIGHT39 {n31Sg * 1000:0})");

        // 2. GROUND CLICKS on the THUNDERCLAP: the volley's step after the press
        var lat = new List<string>();
        bool clicksOk = true;
        int worst = 0, lag60 = -1;
        foreach (int ms in new[] { 40, 60, 80, 100, 120, 140 })
        {
            int click = Steps(ms / 1000.0);
            var c = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + click, lead + Steps(1));
            int lag = c.fireSteps.Count > 0 ? c.fireSteps[0] - lead : -1;
            if (ms == 60) lag60 = lag;
            worst = Math.Max(worst, lag);
            lat.Add($"{ms} ms -> +{lag} ({Ms(lag)} ms, w {c.corrW.ToString("0.00", ci)})");
            clicksOk &= c.fireSteps.Count == 1 && lag == click && c.charged == 0 && c.starts == 0 && c.hits >= 1 && c.groundPresses == 1;
        }
        Console.WriteLine($"  GROUND CLICKS (click -> the volley's step after the press, correction weight): {string.Join("; ", lat)}");
        Check(clicksOk, $"THUNDERCLAP on the ground: a click of 40-140 ms fires ONE volley ON ITS RELEASE STEP, no charge, it hits (60 ms click -> {Ms(lag60)} ms after the press; worst {Ms(worst)} ms, at most shotgunTapS)");

        // 3. GROUND HOLD 1.4 s: no volley while held, one slug on the release
        int hold = Steps(1.4);
        var h = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + hold, lead + hold + Steps(0.3));
        Console.WriteLine($"  GROUND HOLD {hold} steps (1.4 s): fires at [{Rel(h.fireSteps)}], slug {h.charged} at +{(h.firstCharged >= 0 ? h.firstCharged - lead : -1)}, charge35Start at +{(h.firstStart >= 0 ? h.firstStart - lead : -1)}, charge40Press {h.presses40}");
        Check(h.fires == 1 && h.charged == 1 && h.firstCharged == lead + hold, $"THUNDERCLAP on the ground: a 1.4 s hold fires NO volley first: its only shot is the slug, on the release step +{hold} ([{Rel(h.fireSteps)}])");
        Check(h.starts == 1 && h.firstStart - lead == Steps(sgTap), $"THUNDERCLAP on the ground: CHARGING starts past shotgunTapS (at +{(h.firstStart >= 0 ? h.firstStart - lead : -1)}, want +{Steps(sgTap)})");
        Check(h.presses40 == 1, $"THUNDERCLAP on the ground: the press answers at once with charge40Press (the view's / GG's sound cue; {h.presses40})");

        // 4. GROUND SHORT HOLD 0.5 s: a partial charge
        int sh = Steps(0.5);
        var ps = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + sh, lead + sh + Steps(1));
        Console.WriteLine($"  GROUND SHORT HOLD {sh} steps (0.5 s): fires at [{Rel(ps.fireSteps)}], slug {ps.charged}");
        Check(ps.fires == 1 && ps.charged == 0 && ps.fireSteps[0] == lead + sh, $"THUNDERCLAP on the ground: a partial charge's release fires the volley on the release step ([{Rel(ps.fireSteps)}], slug {ps.charged})");

        // 5. AIR: a 60 ms click fires on its press step; the boost acts on that step (looking straight down)
        Func<int, bool> airAll = s => s >= lead - Steps(0.25);
        int c60 = Steps(0.06);
        var ac = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + c60, lead + Steps(1), null, airAll, -89);
        int alag = ac.fireSteps.Count > 0 ? ac.fireSteps[0] - lead : -1;
        Console.WriteLine($"  AIR 60 ms click (looking down): fires at [{Rel(ac.fireSteps)}], vy {ac.preVy.ToString("0.00", ci)} -> {ac.pressVy.ToString("0.00", ci)} m/s on the press step; air presses {ac.airPresses}, ground {ac.groundPresses}");
        Check(ac.fireSteps.Count == 1 && alag == 0 && ac.airPresses == (on ? 1 : 0) && ac.groundPresses == 0, $"THUNDERCLAP in the air: a click fires ON its press step (+{alag}: {Ms(Math.Max(0, alag))} ms added)");
        Check(ac.pressVy - ac.preVy > 5, $"THUNDERCLAP in the air: the boost acts on the press step (vy +{(ac.pressVy - ac.preVy).ToString("0.00", ci)} m/s)");
        var ah = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + hold, lead + hold + Steps(0.3), null, airAll);
        Console.WriteLine($"  AIR HOLD 1.4 s: fires at [{Rel(ah.fireSteps)}], slug {ah.charged} at +{(ah.firstCharged >= 0 ? ah.firstCharged - lead : -1)}");
        Check(ah.Normal.Count == 1 && ah.Normal[0] == lead && ah.charged == 1 && ah.firstCharged == lead + hold, $"THUNDERCLAP in the air: a hold keeps tap36 (the volley on the press, the slug on the release: [{Rel(ah.fireSteps)}])");

        // 6. PRESSED 3 STEPS (L-053): a Player press repeats `pressed` on every sim step of its frame
        Func<int, bool> pr3 = s => s >= lead && s < lead + 3;
        var g3 = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + c60, lead + Steps(1), pr3);
        var a3 = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + c60, lead + Steps(1), pr3, airAll);
        Console.WriteLine($"  PRESSED 3 STEPS ({c60}-step click): ground fires at [{Rel(g3.fireSteps)}] (presses {g3.groundPresses}); air fires at [{Rel(a3.fireSteps)}] (presses {a3.airPresses})");
        Check(g3.fires == 1 && g3.fireSteps[0] == lead + c60 && g3.charged == 0, $"THUNDERCLAP on the ground: pressed for 3 steps is ONE press: one volley, on the release ([{Rel(g3.fireSteps)}])");
        Check(a3.fires == 1 && a3.fireSteps[0] == lead, $"THUNDERCLAP in the air: pressed for 3 steps is ONE press: one volley, on the press ([{Rel(a3.fireSteps)}])");

        // 7. CORRECTION: an 80 ms ground click while the look swings 3 deg a step away from the husk
        int cl = Steps(0.08);
        Func<int, double> swing = s => s > lead && s < lead + cl + 20 ? 3.0 * Math.Min(s - lead, cl) : 0;
        var cr = C40Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + cl, lead + Steps(0.6), null, null, null, swing);
        Console.WriteLine($"  CORRECTION ({cl}-step ground click, the look 3 deg/step off the husk, {3 * cl} deg at the release): volley at [{Rel(cr.fireSteps)}], {cr.firstShotOffPressDeg.ToString("0.00", ci)} deg off the press's aim, {cr.firstShotOffReleaseDeg.ToString("0.00", ci)} deg off the release's, weight {cr.corrW.ToString("0.00", ci)}");
        Check(cr.firstShotOffPressDeg >= 0 && cr.firstShotOffPressDeg < 0.5 && cr.corrected == 1, $"THUNDERCLAP on the ground: the release volley leaves along the PRESS's aim ({cr.firstShotOffPressDeg.ToString("0.00", ci)} deg off it)");

        // 8. COOLING: a slug, then a ground click 1.7 s after the release (past the gun's ~1.52 s interval, inside the slug's 3 s cooldown)
        int click2 = lead + hold + Steps(1.7);
        var co = C40Run(cfg, map, "shotgun", on, s => (s >= lead && s < lead + hold) || (s >= click2 && s < click2 + c60), click2 + Steps(0.5));
        Console.WriteLine($"  COOLING: a 1.4 s hold, then a 60 ms click at +{click2 - lead}: fires at [{Rel(co.fireSteps)}], slug {co.charged}");
        Check(co.charged == 1 && co.fireSteps.Contains(click2), $"THUNDERCLAP on the ground: a click while the slug cools fires ON its press step +{click2 - lead} ([{Rel(co.fireSteps)}])");

        Console.WriteLine(fails.Count == 0 ? "n40weapons GREEN" : $"n40weapons RED ({fails.Count}): {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
