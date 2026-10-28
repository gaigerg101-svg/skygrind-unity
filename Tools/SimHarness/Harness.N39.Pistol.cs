// NIGHT39 builder P1 (the pistol): P1's headless judges. ONE entry method, N39Pistol(cmd, args, ref code) (declared and called in Program.cs).
//   n39pistol [--off39]  design.n39.p39 on the KESTREL (forced on unless --off39; design.n31.charge + n35.charge35 + n36.tap36 on, the n27 row off), a
//                        husk 4 m ahead, scripted RT:
//                        CLICKS 40-140 ms: ONE normal shot ON THE RELEASE STEP, no charge: the tap's added latency per click length (= the click);
//                        HOLD 0.8 s: NO shot while held (no tap first), ONE charged shot on the release step, CHARGING only past tapS;
//                        SHORT HOLD 0.3 s (a partial): the tap on the release (partialFires), no charged shot;
//                        COOLING: a click 0.5 s after a charged shot fires ON its press step (no hold can charge, so no latency);
//                        HELD 4 s at real husks: the full charge fires itself (holdMaxS), the hold keeps killing (never stalls);
//                        CORRECTION: an 80 ms click while the look swings 3 deg a step off the husk: the tap leaves along the PRESS's aim and hits;
//                        the same click with the correction zeroed misses (the RED side of the correction, in the same run);
//                        THUNDERCLAP: a 60 ms click still fires ON the press step (tap36 kept).
//                        GREEN = every PASS line with p39 on. `--off39`: tap36 (the KESTREL's tap on the press): the release lines go RED (L-014's RED).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N39Pistol(string cmd, string[] args, ref int code)
    {
        if (cmd == "n39pistol") N39PistolCommand(args, ref code);
        else if (cmd == "n39reels") N39ReelsCommand(args, ref code);
    }

    /// <summary>
    /// n39reels: the Player reels' KESTREL rules (FeelReel.N35.Verbs rows 25 / 27, FeelReel.N36.Guns' tap36 TAP / HOLD), each judged headless in BOTH
    /// states in one run (p39 forced on, then off: tap36), with the reels' own inputs (a 0.1 s click; a 0.9 s hold; a 0.06 s tap; a 0.8 s hold) and
    /// the reels' own predicates as rewritten for p39 (steps here; the reels count frames). GREEN = every line PASS in both states.
    /// </summary>
    static void N39ReelsCommand(string[] args, ref int code)
    {
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
        int lead = Steps(0.5), full = Steps(cfg.Num("design.n31.charge.revolver.fullSeconds"));
        Console.WriteLine($"n39reels: the reels' KESTREL rules in both states (p39 forced on / off; tap36 on); full {full} steps");
        foreach (bool on in new[] { true, false })
        {
            string st = on ? "p39 ON " : "p39 OFF";
            // verbs row 25: a 0.1 s click; the first shot's steps after the press
            int c25 = Steps(0.1);
            var r25 = P39Run(cfg, map, "revolver", on, s => s >= lead && s < lead + c25, lead + Steps(0.4), false, int.MaxValue);
            int lag = r25.fireSteps.Count > 0 ? r25.fireSteps[0] - lead : -1;
            bool ok25 = r25.fires > 0 && (on ? lag >= 3 && lag < full : lag >= 0 && lag <= 2);
            Check(ok25, $"{st} verbs row 25: a 0.1 s click fires {(on ? "on the release (want 3.." + (full - 1) + ")" : "on the press (want 0..2)")}: first shot +{lag}, {r25.fires} shot(s)");
            // verbs row 27: a 0.9 s hold, released; normal shots before the first charged one, one charge start, one charged shot
            int h27 = Steps(0.9);
            var r27 = P39Run(cfg, map, "revolver", on, s => s >= lead && s < lead + h27, lead + h27 + Steps(0.8), false, int.MaxValue);
            int before = r27.chargedSteps.Count > 0 ? r27.fireSteps.FindAll(x => x < r27.chargedSteps[0]).Count : r27.fires;
            int want = on ? 0 : 1;
            Check(r27.charged == 1 && r27.starts == 1 && before == want, $"{st} verbs row 27: a 0.9 s hold: normal before the charge {before} (want {want}), charge starts {r27.starts}, charged {r27.charged}");
            // tap36 reel TAP: a 0.06 s tap; 1 shot, no charge start, on the press (off) / on the release (on)
            int t = Steps(0.06);
            var rt = P39Run(cfg, map, "revolver", on, s => s >= lead && s < lead + t, lead + Steps(0.6), false, int.MaxValue);
            int tl = rt.fireSteps.Count > 0 ? rt.fireSteps[0] - lead : -1;
            Check(rt.fires == 1 && rt.starts == 0 && rt.charged == 0 && (on ? tl == t : tl == 0), $"{st} tap36 reel TAP: 0.06 s: {rt.fires} shot at +{tl} (want +{(on ? t : 0)}: the {(on ? "release" : "press")}), charge35Start {rt.starts}");
            // tap36 reel HOLD: a 0.8 s hold; the press shot (off: 1, on: 0), 0 while held, 1 charged on the release
            int h = Steps(0.8);
            var rh = P39Run(cfg, map, "revolver", on, s => s >= lead && s < lead + h, lead + h + Steps(0.4), false, int.MaxValue);
            int pressShots = rh.fireSteps.FindAll(x => x <= lead + 1).Count, held = rh.fireSteps.FindAll(x => x > lead + 1 && x < lead + h).Count;
            Check(pressShots == (on ? 0 : 1) && held == 0 && rh.charged == 1 && rh.firstCharged == lead + h && rh.starts == 1, $"{st} tap36 reel HOLD: 0.8 s: press shots {pressShots} (want {(on ? 0 : 1)}), shots while held {held}, charged {rh.charged} at +{rh.firstCharged - lead}");
        }
        Console.WriteLine(fails.Count == 0 ? "n39reels GREEN" : $"n39reels RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    sealed class P39Result
    {
        public int fires, charged, chargedHits, starts, hits, kills, killsLate, autoReleased, firstStart = -1, firstCharged = -1, presses, missing, corrected;
        public readonly List<int> fireSteps = new List<int>();
        public readonly List<int> chargedSteps = new List<int>();
        public double firstShotOffPressDeg = -1, firstShotOffReleaseDeg = -1, corrW = -1;
        public bool firstShotHit;
    }

    static double AngleDeg(Vec3 a, Vec3 b)
    {
        double c = Vec3.Dot(a, b) / (a.Length * b.Length);
        return Math.Acos(Math.Max(-1, Math.Min(1, c))) * 180 / Math.PI;
    }

    /// <summary>One scripted run: `held(s)` the trigger, `swingDeg(s)` degrees added to the look's yaw, `tweak` runs on charge35 after the run starts.</summary>
    static P39Result P39Run(GameConfig cfg, SurvivalMap map, string gun, bool on, Func<int, bool> held, int steps, bool realHp, int lateFrom,
        Func<int, double> swingDeg = null, Action<Charge35> tweak = null)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { chargeShots27 = false };
        sim.n31Override = new N31Blocks { charge = true };
        sim.n35Override = new N35Blocks { charge35 = true };
        sim.n36Override = new N36Blocks { tap36 = true };
        sim.n39Override = on ? new N39Blocks { p39 = true } : N39Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3601, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        if (tweak != null && sim.charge35 != null) tweak(sim.charge35);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        var r = new P39Result { missing = sim.p39Missing };
        int s = 0;
        Vec3 pressDir = default, releaseDir = default;
        bool firstSeen = false;
        sim.events.On<string>("fire", w => { if (w != gun) return; r.fires++; r.fireSteps.Add(s); });
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { r.charged++; r.chargedSteps.Add(s); r.chargedHits += e.hits; if (r.firstCharged < 0) r.firstCharged = s; });
        sim.events.On<ChargeShot27Event>("charge35Start", e => { r.starts++; if (r.firstStart < 0) r.firstStart = s; });
        sim.events.On<ChargeShot27Event>("p39Press", e => r.presses++);
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
            double y = yaw + (swingDeg != null ? swingDeg(s) * Math.PI / 180 : 0);
            bool down = held(s), prev = s > 0 && held(s - 1);
            double cp = Math.Cos(py);
            var dir = new Vec3(-Math.Sin(y) * cp, Math.Sin(py), -Math.Cos(y) * cp);
            if (down && !prev && pressDir.Length == 0) pressDir = dir;
            if (!down && prev && releaseDir.Length == 0) releaseDir = dir;
            Tick27(sim, y, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && !prev } });
        }
        if (sim.loadout.charge27 != null) r.autoReleased = sim.loadout.charge27.autoReleased;
        if (sim.charge35 != null) r.corrected = sim.charge35.corrected39;
        return r;
    }

    static void N39PistolCommand(string[] args, ref int code)
    {
        bool on = !N39Blocks.ForceOff;
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
        const string p = "design.n39.p39.";
        double tapS = cfg.Num(p + "tapS"), fullCorr = cfg.Num(p + "correctFullS"), fadeCorr = cfg.Num(p + "correctFadeS");
        bool partialFires = cfg.Design("n39.p39.partialFires");
        double fullS = cfg.Num("design.n31.charge.revolver.fullSeconds");
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        Console.WriteLine($"n39pistol: {(on ? "design.n39.p39 ON (forced on here; the row's own enabled is " + cfg.Design("n39.p39.enabled") + ")" : "--off39: tap36 on the KESTREL (the RED side)")}; tapS {tapS} s ({Steps(tapS)} steps), full {fullS} s, partialFires {partialFires}, correction full {fullCorr} s / none {fadeCorr} s; n31.charge + n35.charge35 + n36.tap36 on");
        const string gun = "revolver";
        int lead = Steps(0.5);
        string Rel(List<int> l) => l.Count == 0 ? "none" : string.Join(", ", l.ConvertAll(x => "+" + (x - lead)));

        // 1. CLICKS: the tap's added latency per click length (the shot's step minus the press step)
        Console.WriteLine(" KESTREL (revolver): press step = +0");
        var lat = new List<string>();
        bool clicksOk = true, clicksPress = true;
        int worstLag = 0;
        foreach (int ms in new[] { 40, 60, 80, 100, 120, 140 })
        {
            int click = Steps(ms / 1000.0);
            var c = P39Run(cfg, map, gun, on, s => s >= lead && s < lead + click, lead + Steps(1), false, int.MaxValue);
            int lag = c.fireSteps.Count > 0 ? c.fireSteps[0] - lead : -1;
            worstLag = Math.Max(worstLag, lag);
            lat.Add($"{ms} ms -> +{lag} ({(lag * dt * 1000).ToString("0", ci)} ms, w {c.corrW.ToString("0.00", ci)})");
            clicksOk &= c.fireSteps.Count == 1 && lag == click && c.charged == 0 && c.starts == 0 && c.hits == 1;
            clicksPress &= c.fireSteps.Count == 1 && lag == 0;
        }
        Console.WriteLine($"  CLICKS (click length -> shot step after the press, added latency, correction weight): {string.Join("; ", lat)}");
        Check(clicksOk, $"KESTREL: a click of 40-140 ms fires ONE shot ON ITS RELEASE STEP, no charge, it hits (the added latency = the click, at most tapS {tapS * 1000:0} ms; worst +{worstLag} steps = {(worstLag * dt * 1000).ToString("0", ci)} ms)");
        if (!on) Console.WriteLine($"  (--off39: every click fires on its press step: {clicksPress})");

        // 2. HOLD 0.8 s + release: no tap first, one charged shot on the release
        int hold = Steps(0.8), shortHold = Steps(0.3);
        var h = P39Run(cfg, map, gun, on, s => s >= lead && s < lead + hold, lead + hold + Steps(0.3), false, int.MaxValue);
        Console.WriteLine($"  HOLD {hold} steps (0.8 s): shots at [{Rel(h.fireSteps)}], charged {h.charged} at +{(h.firstCharged >= 0 ? h.firstCharged - lead : -1)} ({h.chargedHits} hit), charge35Start at +{(h.firstStart >= 0 ? h.firstStart - lead : -1)}, p39Press {h.presses}");
        Check(h.fires == 1 && h.charged == 1 && h.firstCharged == lead + hold && h.chargedHits >= 1, $"KESTREL: a 0.8 s hold fires NO tap first: its only shot is the charged one, on the release step +{hold} ([{Rel(h.fireSteps)}], {h.chargedHits} hit)");
        Check(h.starts == 1 && h.firstStart - lead == Steps(tapS), $"KESTREL: CHARGING starts on the first step held past tapS (at +{(h.firstStart >= 0 ? h.firstStart - lead : -1)}, the step held {Steps(tapS) + 1}th; want +{Steps(tapS)})");
        Check(h.presses == 1, $"KESTREL: the press answers at once with p39Press (the view's cue; {h.presses})");

        // 3. SHORT HOLD 0.3 s (a partial charge)
        var sh = P39Run(cfg, map, gun, on, s => s >= lead && s < lead + shortHold, lead + shortHold + Steps(1), false, int.MaxValue);
        Console.WriteLine($"  SHORT HOLD {shortHold} steps (0.3 s): shots at [{Rel(sh.fireSteps)}], charged {sh.charged}, charge35Start {sh.starts}");
        Check(partialFires ? sh.fires == 1 && sh.charged == 0 && sh.fireSteps[0] == lead + shortHold : sh.fires == 0, $"KESTREL: a partial charge's release {(partialFires ? "fires the tap on the release step, no charged shot" : "drains")} ([{Rel(sh.fireSteps)}], charged {sh.charged})");

        // 4. COOLING: a charged shot, then a click 0.5 s later (past the KESTREL's own 0.4 s fire interval, inside charge35's 2.5 s charge cooldown):
        // no hold can charge, so the click fires on its press
        int click2 = lead + hold + Steps(0.5), clickLen = Steps(0.06);
        var co = P39Run(cfg, map, gun, on, s => (s >= lead && s < lead + hold) || (s >= click2 && s < click2 + clickLen), click2 + Steps(0.5), false, int.MaxValue);
        Console.WriteLine($"  COOLING: hold 0.8 s, then a 60 ms click at +{click2 - lead}: shots at [{Rel(co.fireSteps)}], charged {co.charged}");
        Check(co.charged == 1 && co.fireSteps.Contains(click2), $"KESTREL: a click while the charge cools fires ON its press step +{click2 - lead} ([{Rel(co.fireSteps)}])");

        // 5. HELD 4 s at real husks: never stalls
        int late = lead + Steps(1.5);
        var held = P39Run(cfg, map, gun, on, s => s >= lead && s < lead + Steps(4), lead + Steps(4), true, late);
        Console.WriteLine($"  HELD 4 s (real husks): shots at [{Rel(held.fireSteps.GetRange(0, Math.Min(6, held.fireSteps.Count)))}{(held.fireSteps.Count > 6 ? ", ..." : "")}] ({held.fires}), kills {held.kills} ({held.killsLate} after 1.5 s), charged {held.charged} (fired itself {held.autoReleased})");
        Check(held.charged >= 1 && held.autoReleased >= 1 && held.killsLate > 0 && (held.fireSteps.Count == 0 || held.fireSteps[0] > lead + Steps(tapS)), $"KESTREL: a held trigger fires no tap first, the full charge fires itself and the hold keeps killing (first shot +{(held.fireSteps.Count > 0 ? held.fireSteps[0] - lead : -1)}, charged {held.charged}, kills after 1.5 s {held.killsLate})");

        // 5b. the slower interval (design.n39.p39.intervalMult): the held auto-fire's gap after the charge fired itself
        double mult = cfg.Num(p + "intervalMult");
        int wantGap = Steps(cfg.Num("weapons.revolver.fireInterval") * (on ? mult : 1));
        int gap = held.fireSteps.Count >= 3 ? held.fireSteps[2] - held.fireSteps[1] : -1;
        Console.WriteLine($"  INTERVAL: the held auto-fire's gap {gap} steps ({(gap * dt).ToString("0.000", ci)} s; intervalMult {(on ? mult : 1)})");
        Check(gap == wantGap, $"KESTREL: the normal shots' interval is x{(on ? mult : 1)} the web's ({gap} steps, want {wantGap})");

        // 6. CORRECTION: an 80 ms click while the look swings 3 deg a step away from the husk (21 deg off at the release)
        int cl = Steps(0.08);
        Func<int, double> swing = s => s > lead && s < lead + cl + 20 ? 3.0 * Math.Min(s - lead, cl) : 0;
        var cr = P39Run(cfg, map, gun, on, s => s >= lead && s < lead + cl, lead + Steps(0.6), false, int.MaxValue, swing);
        var cz = P39Run(cfg, map, gun, on, s => s >= lead && s < lead + cl, lead + Steps(0.6), false, int.MaxValue, swing, q => { q.p39CorrectFullS = 0; q.p39CorrectFadeS = 0; });
        Console.WriteLine($"  CORRECTION ({cl}-step click, the look 3 deg/step off the husk, {3 * cl} deg at the release): shot at [{Rel(cr.fireSteps)}], {cr.firstShotOffPressDeg.ToString("0.00", ci)} deg off the press's aim, {cr.firstShotOffReleaseDeg.ToString("0.00", ci)} deg off the release's, hit {cr.firstShotHit}, weight {cr.corrW.ToString("0.00", ci)}; correction zeroed: {cz.firstShotOffPressDeg.ToString("0.00", ci)} deg off the press's, hit {cz.firstShotHit}");
        Check(cr.firstShotHit && cr.firstShotOffPressDeg >= 0 && cr.firstShotOffPressDeg < 2 && cr.corrected == 1, $"KESTREL: the release tap leaves along the PRESS's aim and hits ({cr.firstShotOffPressDeg.ToString("0.00", ci)} deg off it, hit {cr.firstShotHit})");
        if (on) Check(!cz.firstShotHit, $"KESTREL: with the correction zeroed the same click misses (hit {cz.firstShotHit}: the correction is what lands it)");

        // 7. THUNDERCLAP keeps tap36: a 60 ms click fires on its press step
        var tc = P39Run(cfg, map, "shotgun", on, s => s >= lead && s < lead + Steps(0.06), lead + Steps(1), false, int.MaxValue);
        Console.WriteLine($"  THUNDERCLAP 60 ms click: shots at [{Rel(tc.fireSteps)}], charged {tc.charged}{(tc.missing > 0 ? "; p39Missing " + tc.missing : "")}");
        Check(tc.fireSteps.Count == 1 && tc.fireSteps[0] == lead && tc.charged == 0, $"THUNDERCLAP: tap36 kept, a click fires ON its press step ([{Rel(tc.fireSteps)}])");

        Console.WriteLine(fails.Count == 0 ? "n39pistol GREEN" : $"n39pistol RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
