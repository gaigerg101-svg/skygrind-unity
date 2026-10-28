// NIGHT42 builder W1 (the weapons): W1's headless judges. ONE entry method, N42Weapons(cmd, args, ref code) (declared and called in Program.cs).
// Every sub-command runs the row OFF (NIGHT41's game: the before) and ON (the after) on the survival map with the game's own design rows
// (only the NIGHT42 block under test forced), and prints both.
//   n42weap gauntlet   G3 gauntlet42: fire held with the PULSE gauntlet against a bag 1.6 m ahead for 8 s: the press-to-next-swing gap
//                      (meleeSwing to meleeSwing). GREEN: OFF 2.5 s (300 steps), ON design.n42.gauntlet42.swingSeconds (1.8 s, 216 steps).
//   n42weap parry      G4 parry42: a DRONE orb fired at 12 m/s from 8 m at the player's chest (every other body removed), ONE press per trial at
//                      every step offset from 0.6 s before the orb's touch to 0.15 s after it; the punch (KESTREL in hand) and the gauntlet's
//                      swing. Prints per mode the parry window (first / last offset that parried, its length) and GG's number: the share of
//                      presses made while the orb was inside 2.5 m of the eye (before its touch) that parried, plus presses just after the
//                      touch. GREEN: ON parries every press inside 2.5 m, both modes, and some press after the touch (the mercy).
//   n42weap charge     G10 + G8 charge42, on the ground at a husk 4 m ahead, the KESTREL and the THUNDERCLAP:
//                      HELD: hold to FULL, release (the charged shot), press again at once and HOLD through the whole cooldown + FULL, release.
//                        Before: the press's tap then auto-fired taps through the cooldown, no new charge. After: the press's tap only, then
//                        the next charge starts the step the cooldown ends (charge42Rearm) and the release fires a second charged shot.
//                      TURN: a charged shot, then a press every 2 steps for 1.5 s: the first shot after the charged one must leave after the
//                        reticle's cannon turn (before: NIGHT40's 1.70 + 0.12 s, read from UnityTuning.Pistol40.cs; after:
//                        design.n42.charge42.cannonTurnS), and every tap after the tap turn (0.28 + 0.06 s; reticle42's from
//                        UnityTuning.Reticle42.cs). Prints how many shots left inside a running turn.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N42Weapons(string cmd, string[] args, ref int code)
    {
        if (cmd != "n42weap") return;
        string what = args.Length > 1 ? args[1] : "all";
        int fails = 0;
        if (what == "gauntlet" || what == "all") fails += W42Gauntlet();
        if (what == "parry" || what == "all") fails += W42Parry();
        if (what == "charge" || what == "all") fails += W42Charge();
        Console.WriteLine($"n42weap {what} {(fails == 0 ? "GREEN" : "RED")}");
        code = fails == 0 ? 0 : 1;
    }

    /// <summary>A headless survival run with the game's design rows and the NIGHT42 blocks `b` (the before: N42Blocks.Off).</summary>
    static SurvivalSim W42Sim(GameConfig cfg, SurvivalMap map, N42Blocks b, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n42Override = b;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    // ---- G3 gauntlet42 -----------------------------------------------------------------------------------------------------------------------

    static (List<int> swings, double dmg) W42GauntletRun(GameConfig cfg, SurvivalMap map, bool on)
    {
        SurvivalSim sim = W42Sim(cfg, map, on ? new N42Blocks { gauntlet42 = true } : N42Blocks.Off, 4201);
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        int s = 0;
        var swings = new List<int>();
        double dmg = 0;
        sim.events.On<MeleeEvent>("meleeSwing", e => { if (e.weapon == "gauntlet") swings.Add(s); });
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") dmg = e.damage; });
        Husk h = null;
        for (s = 0; s < (int)(8 / sim.step); s++)
        {
            h = Pin27(sim, h, sp, ux, uz, 1.6, 1e9);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = s == 0 } });
        }
        return (swings, dmg);
    }

    static int W42Gauntlet()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        int want27 = MovementMath.StepsFor(cfg.Num("design.n27.pulse27.swingSeconds"), dt);
        int want42 = MovementMath.StepsFor(cfg.Num("design.n42.gauntlet42.swingSeconds"), dt);
        int fails = 0;
        foreach (bool on in new[] { false, true })
        {
            var (sw, dmg) = W42GauntletRun(cfg, map, on);
            var gaps = new List<int>();
            for (int i = 1; i < sw.Count; i++) gaps.Add(sw[i] - sw[i - 1]);
            int want = on ? want42 : want27;
            bool ok = gaps.Count > 0 && gaps.TrueForAll(g => g == want);
            if (!ok) fails++;
            string gs = gaps.Count == 0 ? "none" : string.Join(",", gaps);
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} gauntlet42 {(on ? "ON " : "OFF")}: {sw.Count} swings in 8 s, press-to-next-swing gaps {gs} steps = {(gaps.Count > 0 ? gaps[0] * dt : 0):0.000} s (want {want} = {want * dt:0.000} s); {dmg:0} dmg a hit, {(gaps.Count > 0 ? dmg / (gaps[0] * dt) : 0):0.0} dps");
        }
        return fails;
    }

    // ---- G10 + G8 charge42 ------------------------------------------------------------------------------------------------------------------

    sealed class W42ChargeResult
    {
        public readonly List<int> normal = new List<int>(), charged = new List<int>(), rearm = new List<int>();
        public int heldSteps;
    }

    /// <summary>One scripted ground run with `gun` against a husk 4 m ahead: `held(s)` the trigger (a press on each rising edge).</summary>
    static W42ChargeResult W42ChargeRun(GameConfig cfg, SurvivalMap map, string gun, bool on, Func<int, bool> held, Func<int, bool> pressed, int steps)
    {
        SurvivalSim sim = W42Sim(cfg, map, on ? new N42Blocks { charge42 = true } : N42Blocks.Off, 4203);
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        var r = new W42ChargeResult();
        int s = 0;
        bool chargedNow = false;
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (e.gun != gun) return; r.charged.Add(s); chargedNow = true; });
        sim.events.On<string>("fire", w => { if (w != gun) return; if (chargedNow) return; r.normal.Add(s); });
        sim.events.On<ChargeShot27Event>("charge42Rearm", e => { if (e.gun == gun) r.rearm.Add(s); });
        Husk body = null;
        for (s = 0; s < steps; s++)
        {
            chargedNow = false;
            body = Pin27(sim, body, sp, ux, uz, 4.0, 1e9);
            double py = 0;
            if (body != null && body.alive) (yaw, py) = ArclineLook(sim.Eye, body.AimPoint);
            bool down = held(s), prev = s > 0 && held(s - 1);
            bool pr = pressed != null ? pressed(s) : down && !prev;
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = pr } });
        }
        if (sim.charge35 != null) r.heldSteps = sim.charge35.c42Held;
        return r;
    }

    static double W42Tuning(string file, string name, double fallback)
    {
        try
        {
            string src = System.IO.File.ReadAllText(System.IO.Path.Combine(RepoRoot, "Assets/Scripts/Game/" + file));
            var m = System.Text.RegularExpressions.Regex.Match(src, name + @" = ([0-9.]+)f");
            return m.Success ? double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : fallback;
        }
        catch (System.IO.IOException) { return fallback; }
    }

    static int W42Charge()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        string S(int steps) => (steps * dt).ToString("0.000", ci);
        int fails = 0;
        double[] fullS = { cfg.Num("design.n40.charge40.revolverFullS"), cfg.Num("design.n40.charge40.shotgunFullS") };
        double[] coolS = { cfg.Num("design.n35.charge35.revolver.cooldownS"), cfg.Num("design.n35.charge35.shotgun.cooldownS") };
        string[] guns = { "revolver", "shotgun" };
        int lead = Steps(0.5);
        Console.WriteLine($"n42weap charge: FULL {fullS[0]} / {fullS[1]} s, the charge cooldown {coolS[0]} / {coolS[1]} s (KESTREL / THUNDERCLAP), holdMaxS {cfg.Num("design.n35.charge35.holdMaxS")}; on the ground, a husk 4 m ahead");
        // HELD (G10)
        for (int g = 0; g < 2; g++)
        {
            int full = Steps(fullS[g]) + 6, cool = Steps(coolS[g]);
            int rel1 = lead + full, press2 = rel1 + 2, rel2 = press2 + cool + Steps(fullS[g]) + 12;
            Func<int, bool> held = s => (s >= lead && s < rel1) || (s >= press2 && s < rel2);
            foreach (bool on in new[] { false, true })
            {
                var r = W42ChargeRun(cfg, map, guns[g], on, held, null, rel2 + Steps(0.4));
                // the press's own tap: the first normal shot within one gun interval (0.6 / 1.7 s) of the re-press (it waits for the gun's interval); the rest auto-fired while held
                int pressTap = r.normal.FindAll(x => x >= press2 && x < press2 + Steps(g == 0 ? 0.6 : 1.7)).Count > 0 ? 1 : 0;
                int tapsHeld = r.normal.FindAll(x => x >= press2 && x < rel2).Count - pressTap;
                int charged2 = r.charged.FindAll(x => x >= press2).Count;
                bool ok = !on || (tapsHeld == 0 && r.rearm.Count == 1 && charged2 == 1);
                if (!ok) fails++;
                string rearm = r.rearm.Count == 0 ? "none" : string.Join(",", r.rearm.ConvertAll(x => "+" + S(x - press2) + " s"));
                Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} HELD {guns[g]} {(on ? "ON " : "OFF")}: charged shots {r.charged.Count} (after the re-press {charged2}); the re-press's tap {pressTap}; shots auto-fired while held through the cooldown {tapsHeld}; held-start of the next charge {rearm}; steps held silent {r.heldSteps}");
            }
        }
        // TURN (G8): the KESTREL
        double cannonOff = Skygrind.Game.UnityTuning.Pistol40CannonSpinS + Skygrind.Game.UnityTuning.Pistol40CannonSnapS;
        double tapOff = Skygrind.Game.UnityTuning.Pistol40TapSpinS + Skygrind.Game.UnityTuning.Pistol40TapSnapS;
        double cannonOn = cfg.Num("design.n42.charge42.cannonTurnS");
        double tapOn = W42Tuning("UnityTuning.Reticle42.cs", "Reticle42TapSpinS", -1) + W42Tuning("UnityTuning.Reticle42.cs", "Reticle42TapSnapS", 0);
        {
            int full = Steps(fullS[0]) + 6, rel1 = lead + full, end = rel1 + Steps(1.5);
            Func<int, bool> held = s => (s >= lead && s < rel1) || (s > rel1 && s < end && (s - rel1) % 2 == 1);
            foreach (bool on in new[] { false, true })
            {
                var r = W42ChargeRun(cfg, map, "revolver", on, held, null, end);
                double cannon = on ? cannonOn : cannonOff, tap = on ? tapOn : tapOff;
                int inside = 0, firstAfter = -1;
                int turnEnd = r.charged.Count > 0 ? r.charged[0] + Steps(cannon) : -1;
                int last = r.charged.Count > 0 ? r.charged[0] : -1;
                foreach (int x in r.normal)
                {
                    if (x <= last) continue;
                    if (firstAfter < 0) firstAfter = x;
                    if (x < turnEnd) inside++;
                    turnEnd = x + Steps(tap);
                }
                bool ok = !on || (r.charged.Count == 1 && inside == 0 && tap > 0);
                if (!ok) fails++;
                Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} TURN revolver {(on ? "ON " : "OFF")}: cannon turn {cannon:0.00} s, tap turn {tap:0.00} s; the first tap {(firstAfter < 0 || r.charged.Count == 0 ? "none" : "+" + S(firstAfter - r.charged[0]) + " s")} after the charged shot; taps in 1.5 s {r.normal.FindAll(x => x > rel1).Count}; shots fired inside a running turn {inside}");
            }
        }
        return fails;
    }

    // ---- G4 parry42 --------------------------------------------------------------------------------------------------------------------------

    /// <summary>A horizontal direction from the eye with nothing solid for `dist` + 1 m at eye and chest height: (yaw, unit x, unit z).</summary>
    static (double yaw, double dx, double dz) W42OpenDir(SurvivalSim sim, double dist)
    {
        Vec3 eye = sim.Eye;
        for (int k = 0; k < 64; k++)
        {
            double a = k * Math.PI / 32;
            double dx = Math.Sin(a), dz = -Math.Cos(a);
            bool clear = true;
            foreach (double h in new[] { 0.0, -0.4, -0.8 })
            {
                var from = new Vec3(eye.x, eye.y + h, eye.z);
                if (sim.world.RayIntersect(from, new Vec3(dx, 0, dz), out RayHit hit) && hit.distance < dist + 1) clear = false;
            }
            if (clear) return (Math.Atan2(-dx, -dz), dx, dz);
        }
        return (double.NaN, 0, -1);
    }

    sealed class W42ParryTrial
    {
        public int offset;        // the press's step minus the touch step (negative: before the touch)
        public double eyeDist;    // the orb's centre to the eye at the press (-1: the orb was gone)
        public bool parried, buffered;
    }

    /// <summary>One mode ("punch" | "swing"), the row on or off: the touch step (no press) and one trial per press offset.</summary>
    static (int touch, List<W42ParryTrial> trials, string note) W42ParryRun(GameConfig cfg, SurvivalMap map, bool on, string mode, int fromOff, int toOff)
    {
        SurvivalSim sim = W42Sim(cfg, map, on ? new N42Blocks { parry42 = true } : N42Blocks.Off, 4202);
        sim.mover.invulnerable = false;
        string gun = mode == "swing" ? "gauntlet" : "revolver";
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        Core36Keep.Clear();
        DroneOrbs orbs = sim.husks.orbs;
        double dt = sim.step;
        const double from = 8, speed = 12;
        double yaw = 0, ux = 0, uz = -1;
        bool first = true;
        int reflects0 = 0, buffered0 = 0;
        // the player pinned 30 m over the spawn, still (open sky: no wall or floor ends an orb early)
        Vec3 hang = new Vec3(map.playerSpawn.x, map.playerSpawn.y + 30, map.playerSpawn.z);
        void Hang()
        {
            sim.mover.position = hang;
            sim.mover.velocity = new Vec3(0, 0, 0);
            sim.mover.grounded = false;
        }
        // one idle step with the gun in hand, then settle
        void Idle(int n, bool pick)
        {
            for (int i = 0; i < n; i++)
            {
                sim.health.hp = sim.health.max;
                Hang();
                Core36Tick(sim, yaw, 0, new CombatInput { weapon = pick && i == 0 ? slot : -1 });
            }
        }
        Idle(30, true);
        (yaw, ux, uz) = W42OpenDir(sim, from);
        string note = double.IsNaN(yaw) ? "no open line" : "";
        if (double.IsNaN(yaw)) yaw = 0;
        // one trial: fire, press at step `pressAt` from the fire (-1: none); returns (touch step or -1, the press's eye distance, parried, buffered)
        (int touchStep, double dist, bool parried, bool buf) Trial(int pressAt)
        {
            Idle(mode == "swing" ? 330 : 150, false); // every cooldown and buffer runs out (the swing's 2.5 s)
            orbs.Clear();
            Vec3 eye = sim.Eye;
            var src = new Vec3(eye.x + ux * from, eye.y - 0.4, eye.z + uz * from);
            var aim = new Vec3(eye.x, eye.y - 0.4, eye.z);
            DroneOrbs.Orb orb = orbs.Fire(src, Vec3.Normalize(aim - src), null, speed);
            reflects0 = orbs.reflects;
            buffered0 = sim.loadout.orbBufferParries42;
            int touch = -1;
            double dist = -1;
            for (int s = 0; s < 140; s++)
            {
                bool press = s == pressAt;
                if (press) dist = orb.alive && !orb.reflected ? Vec3.DistanceTo(orb.position, sim.Eye) : -1;
                sim.health.hp = sim.health.max;
                var c = new CombatInput { weapon = -1 };
                if (press && mode == "swing") c.fire = new Button { pressed = true, held = true };
                if (press && mode == "punch") c.punch = new Button { pressed = true, held = true };
                Hang();
                Core36Tick(sim, yaw, -0.15, c);
                if (touch < 0 && !orb.reflected && (!orb.alive || orb.mercy42 > 0 || orb.dodged)) touch = s;
                if (orb.reflected && s > pressAt + 30) break;
            }
            return (touch, dist, orbs.reflects > reflects0, sim.loadout.orbBufferParries42 > buffered0);
        }
        string why = null;
        sim.events.On<DroneOrbs.DestroyedEvent>("orbDestroyed", e => { if (why == null) why = e.reason; });
        var cal = Trial(-1);
        int touchAt = cal.touchStep;
        if (why != null && why != "player") note += $" (the calibration orb ended: {why})";
        var trials = new List<W42ParryTrial>();
        if (touchAt < 0) return (-1, trials, note + " (the orb never touched)");
        for (int o = fromOff; o <= toOff; o++)
        {
            int at = touchAt + o;
            if (at < 0) continue;
            var t = Trial(at);
            trials.Add(new W42ParryTrial { offset = o, eyeDist = t.dist, parried = t.parried, buffered = t.buf });
        }
        first = false;
        return (touchAt, trials, note);
    }

    static int W42Parry()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        int fromOff = -MovementMath.StepsFor(0.6, dt), toOff = MovementMath.StepsFor(0.15, dt);
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        string Ms(int steps) => (steps * dt * 1000).ToString("0", ci);
        int fails = 0;
        Console.WriteLine($"n42weap parry: orb.reflectRange {cfg.Num("orb.reflectRange")} m; parry42 reachMult {cfg.Num("design.n42.parry42.reachMult")}, mercyS {cfg.Num("design.n42.parry42.mercyS")}, orbBufferS {cfg.Num("design.n42.parry42.orbBufferS")}; parry36 bufferS {cfg.Num("design.n36.parry36.bufferS")} (enabled {cfg.Design("n36.parry36.enabled")}); an orb at 12 m/s from 8 m, one press per trial at offsets {Ms(fromOff)}..+{Ms(toOff)} ms from its touch");
        foreach (string mode in new[] { "punch", "swing" })
        {
            foreach (bool on in new[] { false, true })
            {
                var (touch, trials, note) = W42ParryRun(cfg, map, on, mode, fromOff, toOff);
                if (touch < 0) { Console.WriteLine($"  RED  {mode} {(on ? "ON " : "OFF")}: {note}"); fails++; continue; }
                int firstOk = int.MaxValue, lastOk = int.MinValue, okCount = 0, inReach = 0, inReachOk = 0, after = 0, afterOk = 0, buf = 0;
                foreach (var t in trials)
                {
                    if (t.parried) { okCount++; firstOk = Math.Min(firstOk, t.offset); lastOk = Math.Max(lastOk, t.offset); if (t.buffered) buf++; }
                    if (t.offset < 0 && t.eyeDist >= 0 && t.eyeDist <= 2.5) { inReach++; if (t.parried) inReachOk++; }
                    if (t.offset >= 0) { after++; if (t.parried) afterOk++; }
                }
                string win = okCount == 0 ? "none" : $"{Ms(firstOk)}..{(lastOk >= 0 ? "+" : "")}{Ms(lastOk)} ms ({okCount} presses = {Ms(okCount)} ms)";
                bool ok = !on || (inReach > 0 && inReachOk == inReach && afterOk > 0);
                if (!ok) fails++;
                Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} {mode} {(on ? "ON " : "OFF")}: touch at step {touch} of the flight; parry window {win}; inside 2.5 m before the touch {inReachOk}/{inReach} parried ({(inReach > 0 ? 100.0 * inReachOk / inReach : 0):0} %, missed {inReach - inReachOk}); after the touch {afterOk}/{after}; by the orb buffer {buf} {note}");
            }
        }
        return fails;
    }
}
