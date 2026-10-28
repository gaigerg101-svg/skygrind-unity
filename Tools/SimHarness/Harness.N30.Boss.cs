// NIGHT30 builder B: `seam30` (the ring seam measured with the real Mover) and `boss30 <lantern|glacier|wyrm|loom> [--seeds n] [--first n] [--dbg]` (the NIGHT30 boss rules, headless).
// Declared and called by the lead's Program.cs (N30Boss). Each boss row prints RED (its design.n30 row off) and GREEN (on) with the kill time of a rider / chaser lag bot and of a camper.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N30Boss(string cmd, string[] args, ref int code)
    {
        if (cmd != "seam30" && cmd != "boss30") return;
        code = 0;
        if (cmd == "seam30") { code = Seam30(args); return; }
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = 6, first = 1;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") int.TryParse(args[i + 1], out seeds);
            if (args[i] == "--first") int.TryParse(args[i + 1], out first);
        }
        bool dbg = Array.IndexOf(args, "--dbg") >= 0;
        sets30.Clear();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--set") sets30.Add(args[i + 1]);
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        try
        {
            if (which == "all" || which == "lantern") code |= Lantern30Command(seeds, first, dbg);
            if (which == "all" || which == "wyrm") code |= Wyrm30Command(seeds, first, dbg);
            if (which == "all" || which == "glacier") code |= Glacier30Command(seeds, first, dbg);
            if (which == "all" || which == "loom") code |= Loom30Command(dbg);
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
    }

    // --- seam30 -------------------------------------------------------------------------------------------------------------------------

    /// <summary>The ring seam (DESIGN30 I-4): the real Mover rides a flat panel of a closed ring at entry speed into the next panel's corner (22.5 degrees for 16 panels,
    /// 30 for 12); prints whether the SAME ride survives the seam, the speed kept, and whether a new ride re-attaches when it does not.</summary>
    static bool seamTrace;

    static int Seam30(string[] args)
    {
        seamTrace = Array.IndexOf(args, "--trace") >= 0;
        GameConfig cfg = Config();
        double dt = cfg.Dt;
        Console.WriteLine($"seam30: the Mover's corner rule {cfg.movement.wallRide.cornerAngleDeg} deg, ride entry {cfg.movement.wallRide.entrySpeed} m/s, min {cfg.movement.wallRide.minSpeed} m/s, max ride {cfg.movement.wallRide.maxTime} s");
        int fails = 0;
        foreach (int panels in new[] { 16, 12 })
        {
            foreach (double speed in new[] { 12.0, 16.0 })
            {
                SeamResult r = SeamRun(cfg, panels, speed, dt);
                bool clean = r.seams.Count > 0 && r.seams[0].sameRide && r.seams[0].after >= 0.9 * r.seams[0].before;
                string verdict = r.seams.Count == 0 ? "NO SEAM REACHED" : r.seams[0].sameRide ? (clean ? "SURVIVES cleanly" : "SURVIVES, slows") : "BREAKS";
                var parts = new List<string>();
                foreach (SeamCross s in r.seams) parts.Add($"seam at t {s.t:0.00} s: {(s.sameRide ? "same ride" : s.ridingAfter ? "new ride" : "ride lost")} {s.before:0.0} -> {s.after:0.0} m/s ({100 * s.after / Math.Max(1e-9, s.before):0} %)");
                Console.WriteLine($"seam30 {panels} panels ({360.0 / panels:0.0} deg seam) at {speed:0} m/s: {verdict}; ride started t {r.rideStart:0.00} s, ended '{r.firstEnd}' at t {r.rideEnd:0.00} s; {string.Join("; ", parts)}");
            }
        }
        Console.WriteLine("seam30: the ring uses design.n30.lantern30.arena.panels (16 by default; 12 only if the 30 degree seam survives cleanly)");
        return fails;
    }

    sealed class SeamCross
    {
        public double t, before, after;
        public bool sameRide, ridingAfter;
    }

    sealed class SeamResult
    {
        public readonly List<SeamCross> seams = new List<SeamCross>();
        public string firstEnd = "none";
        public double rideStart = -1, rideEnd = -1;
    }

    static SeamResult SeamRun(GameConfig cfg, int panels, double speed, double dt)
    {
        var prm = new Lantern30ArenaParams { panels = panels, apothem = 15, height = 14, platformTop = 2.4, platformDepth = 2.4, platformInset = 0.5, laneLow = 2, laneHigh = 6, ventRadius = 9, platformPanels = new int[0], ventPanels = new int[0] };
        var arena = new Lantern30Arena(prm);
        var w = new TriangleWorld();
        arena.FillWorld(w, default);
        var m = new Mover(cfg.movement, w);
        var evs = new List<MoverEvent>();
        m.Events += e => evs.Add(e);
        // start on panel 1, 0.4 of a panel before its centre, touching the wall (the capsule radius 0.35), 4 m up and rising 2 m/s (the rider has jumped onto the lane),
        // moving counter-clockwise; the stick holds forward + into the wall and the view follows the ring's tangent (a player looking along the wall)
        int p0 = 1;
        Vec3 n = arena.InwardNormal(p0), t = arena.Tangent(p0);
        Vec3 at = -n * prm.apothem + n * 0.35 - t * (0.4 * arena.PanelWidth);
        m.Reset(at.x, 4.0, at.z);
        m.velocity = new Vec3(t.x * speed, 2, t.z * speed);
        var r = new SeamResult();
        int ride = 0, lastPanel = p0, settle = 0;
        for (int i = 0; i < (int)(1.5 / dt); i++)
        {
            double ang = JsMath.Atan2(m.position.z, m.position.x);
            Vec3 tan = new Vec3(-JsMath.Sin(ang), 0, JsMath.Cos(ang)), outward = new Vec3(JsMath.Cos(ang), 0, JsMath.Sin(ang));
            double yaw = Math.Atan2(-tan.x, -tan.z);
            Vec3 wishR = MovementMath.WishDirection(1, 1, yaw);
            double side = wishR.x * outward.x + wishR.z * outward.z > 0 ? 1 : -1; // the stick's sideways sign that points INTO the wall
            int before = evs.Count, rideBefore = ride;
            bool ridingBefore = m.state == MoveState.WallRide;
            double vBefore = m.Speed;
            m.Step(new InputFrame { moveZ = 1, moveX = side, yaw = yaw }, dt);
            for (int k = before; k < evs.Count; k++)
            {
                MoverEvent e = evs[k];
                if (e.type == MoverEventType.WallRideStart) { ride++; if (ride == 1) r.rideStart = i * dt; }
                if (e.type == MoverEventType.WallRideEnd && ride == 1 && r.rideEnd < 0) { r.firstEnd = e.reason; r.rideEnd = i * dt; }
            }
            int pan = arena.PanelAt(m.position.x, m.position.z);
            if (seamTrace) Console.WriteLine($"    t {i * dt:0.000} panel {pan} y {m.position.y:0.00} wall {arena.WallDistance(m.position.x, m.position.z):0.00} v {m.Speed:0.0} {m.state}");
            bool riding = m.state == MoveState.WallRide;
            if (pan != lastPanel)
            {
                // the speed AFTER is read a few steps on (the corner's push-out settles over 2-3 steps): the next loop fills it
                r.seams.Add(new SeamCross { t = i * dt, before = vBefore, after = m.Speed, sameRide = ridingBefore && riding && ride == rideBefore, ridingAfter = riding });
                lastPanel = pan;
                settle = 6;
            }
            else if (settle > 0 && --settle == 0 && r.seams.Count > 0)
            {
                SeamCross s = r.seams[r.seams.Count - 1];
                s.after = m.Speed;
                s.sameRide = s.sameRide && riding && ride == rideBefore;
            }
            if (m.grounded) break;
        }
        if (r.rideEnd < 0 && ride >= 1) r.firstEnd = "still riding at 1.5 s or landed";
        return r;
    }

    // --- shared ----------------------------------------------------------------------------------------------------------------------------

    /// <summary>The design config with the named design.n30 blocks' `enabled` set true (the rows stay off in the file until the lead flips them).</summary>
    static GameConfig Cfg30(params string[] on)
    {
        string design = File.ReadAllText(Path.Combine(Root, "..", "Design", "unity-config.json"));
        int n30 = design.IndexOf("\"n30\": {", StringComparison.Ordinal);
        // every boss block is set explicitly (on: the named ones; off: the rest), whatever the file says now (the lead flips rows on for builds)
        foreach (string b in new[] { "lantern30", "glacier30", "wyrm30", "span", "glide" })
        {
            int at = design.IndexOf("\"" + b + "\": {", n30, StringComparison.Ordinal);
            if (at < 0) throw new Exception("no design.n30." + b);
            var m = new System.Text.RegularExpressions.Regex("\"enabled\"\\s*:\\s*(true|false)").Match(design, at);
            if (!m.Success) throw new Exception("no design.n30." + b + ".enabled");
            string want = "\"enabled\": " + (Array.IndexOf(on, b) >= 0 ? "true" : "false");
            design = design.Substring(0, m.Index) + want + design.Substring(m.Index + m.Length);
        }
        // --set block.key=value (a tuning probe: the first `"key": value` after design.n30's block; an absent key is added after the block's "enabled")
        foreach (string kv in sets30)
        {
            string[] a = kv.Split('=');
            string[] bk = a[0].Split('.');
            int b0 = design.IndexOf("\"" + bk[0] + "\": {", n30, StringComparison.Ordinal);
            var rx = new System.Text.RegularExpressions.Regex("(\"" + bk[1] + "\"\\s*:\\s*)[-0-9.a-z]+");
            var mm = rx.Match(design, b0);
            int blockEnd = design.IndexOf("\n      }", b0, StringComparison.Ordinal);
            if (mm.Success && mm.Index < blockEnd) design = design.Substring(0, mm.Index) + mm.Groups[1].Value + a[1] + design.Substring(mm.Index + mm.Length);
            else
            {
                var en = new System.Text.RegularExpressions.Regex("\"enabled\"\\s*:\\s*(true|false)").Match(design, b0);
                design = design.Substring(0, en.Index + en.Length) + ", \"" + bk[1] + "\": " + a[1] + design.Substring(en.Index + en.Length);
            }
        }
        return GameConfig.FromText(File.ReadAllText(Path.Combine(Root, "config.json")), design);
    }

    static readonly List<string> sets30 = new List<string>();

    static double Median30(List<double> v)
    {
        if (v.Count == 0) return 0;
        var c = new List<double>(v);
        c.Sort();
        return c[c.Count / 2];
    }

    // --- boss30 lantern ----------------------------------------------------------------------------------------------------------------------

    sealed class LanternRow30
    {
        public int steps, deaths, rides, sweeps, sweepTicks, sweepDodges, ledFans, lines, tellLines, coreLines, platformHits, fullHits, lightHits, culled, lobs;
        public bool killed, ring;
        public double hpEnd, sweepDamage, floorDamage, hurtBoss;
        public double rainMaxR;
        public string firstLines = "";
    }

    /// <summary>mode 0: the rider (LanternPilot30 on the ring, LanternPilot29 on the courtyard with the row off); 1: the floor camper (immortal, stands at the spawn, pistol
    /// all the time); 2: the platform camper (immortal, stands on the first platform, pistol all the time).</summary>
    static LanternRow30 LanternRun30(GameConfig cfg, uint seed, int capSteps, int mode, bool on, bool dbg, N31Blocks? n31o = null)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { boss1 = true };
        sim.n29Override = new N29Blocks { boss1 = true };
        sim.n30Override = new N30Blocks { lantern30 = on };
        if (n31o.HasValue) sim.n31Override = n31o; // NIGHT31 B: camp31 lantern31 runs the same rider with the NIGHT31 rows
        sim.n26Override = N26Blocks.Off;
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new LanternRow30();
        sim.events.On<BossLine30>("bossLine30", e => { row.lines++; if (e.key == "tell") row.tellLines++; else row.coreLines++; if (row.lines <= 2) row.firstLines += (row.lines > 1 ? " / " : "") + $"'{e.text}' at {sim.lantern27.stepNo * dt:0.0} s"; });
        sim.events.On<HurtEvent>("hurt", e => { if (e.source == "boss") row.hurtBoss += e.amount; });
        if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) { row.steps = -1; return row; }
        Lantern27 l = sim.lantern27;
        Vec3 stand = spawn;
        if (mode == 2 && on)
        {
            Lantern30Arena a = sim.lantern30Arena ?? Lantern30Arena.Build(cfg);
            int k = a.p.platformPanels[0];
            Vec3 lp = -a.InwardNormal(k) * (a.p.apothem - a.p.platformDepth / 2);
            stand = new Vec3(l.OriginWorld.x + lp.x, l.OriginWorld.y + a.p.platformTop + 0.05, l.OriginWorld.z + lp.z);
        }
        sim.Respawn(stand, yaw);
        var si = (ScriptedInput)sim.input;
        var p30 = new LanternPilot30 { hand = new Mulberry32(seed * 7919u + 13u) };
        var p29 = new LanternPilot29 { hand = new Mulberry32(seed * 7919u + 13u) };
        double yawL = yaw * Math.PI / 180;
        int step = 0;
        var charge31 = new LanternCharge31(); // NIGHT31 B: the shared charge-and-release rule (Game/Bot/LanternCharge31.cs), as the Player reel lantern31 drives it
        while (l.Alive && step < capSteps)
        {
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            if (mode == 0)
            {
                InputFrame mv;
                CombatInput cb;
                if (on && sim.lantern30Arena != null)
                {
                    p30.Step(sim, yawL, step); mv = p30.Move; cb = p30.Combat;
                    // NIGHT31 B (camp31 lantern31): with the charge on, LanternCharge31 holds from a ride, releases a FULL charge on the wall with the aim on the lantern
                    if (n31o.HasValue && n31o.Value.charge) cb = charge31.Fire(sim, mv, cb, step);
                }
                else
                {
                    p29.Step(sim, yawL, step);
                    mv = p29.Move; cb = p29.Combat;
                    Vec3 fw = MovementMath.WishDirection(0, 1, mv.yaw);
                    double cp = JsMath.Cos(mv.pitch);
                    Vec3 dir = new Vec3(fw.x * cp, JsMath.Sin(mv.pitch), fw.z * cp);
                    double t = l.Raycast(eye, dir);
                    if (t >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < t)) cb.fire = new Skygrind.Core.Button { held = true, pressed = step % 2 == 0 };
                }
                yawL = mv.yaw;
                si.SetLook(mv.yaw, mv.pitch);
                si.Set(mv, cb);
            }
            else
            {
                Vec3 d = l.Centre - eye;
                double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                si.SetLook(wy, wp);
                int gun = sim.loadout.owned.IndexOf("revolver");
                si.Set(new InputFrame { yaw = wy, pitch = wp }, new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Skygrind.Core.Button { held = true, pressed = step % 2 == 0 } });
            }
            sim.Step();
            step++;
            if (mode != 0) sim.health.hp = sim.health.max; // the campers are IMMORTAL: the rule, not the deaths, must make them slow
            foreach (Lantern27Orb ob in l.orbs) if (ob.kindId == 3) row.rainMaxR = Math.Max(row.rainMaxR, JsMath.Hypot(ob.pos.x - l.OriginWorld.x, ob.pos.z - l.OriginWorld.z));
            if (dbg && step % 1200 == 0) Console.WriteLine($"    t={step * dt:0}s {l.phase} hp {l.HpFrac:0.00} stage {sim.lantern29?.Stage} state {sim.mover.state} php {sim.health.hp:0} sweep {sim.lantern30?.SweepPanel} rides {p30.rides} pos ({sim.mover.position.x - l.OriginWorld.x:0.0},{sim.mover.position.y - l.OriginWorld.y:0.0},{sim.mover.position.z - l.OriginWorld.z:0.0})");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("hulk", out spawn, out yaw);
                sim.Respawn(mode == 2 ? stand : spawn, yaw);
            }
        }
        row.steps = step;
        row.killed = !l.Alive;
        row.hpEnd = l.HpFrac;
        row.rides = on ? p30.rides : p29.rides;
        row.sweepDodges = p30.sweepDodges;
        row.ring = sim.lantern30Arena != null && on;
        if (sim.lantern31 != null && n31o.HasValue) campLantern31Line = $"shield breaks {sim.lantern31.breaks}, sparks {sim.lantern31.sparks}, open hits {sim.lantern31.openHits}, light hits {sim.lantern31.lightHits}, sink state {sim.lantern31.sinkState}; charged hits {sim.lantern31.chargedSeen} (rejected: not from the wall {sim.lantern31.rejectNotWall}, on a platform {sim.lantern31.rejectPlatform}); rider holds {charge31.holds}, releases on aim {charge31.releases}";
        if (sim.lantern29 != null) { row.fullHits = sim.lantern29.fullHits; row.lightHits = sim.lantern29.lightHits; row.floorDamage = sim.lantern29.floorDamage; }
        if (sim.lantern30 != null && on) { row.sweeps = sim.lantern30.sweeps; row.sweepTicks = sim.lantern30.sweepTicks; row.sweepDamage = sim.lantern30.sweepDamage; row.ledFans = sim.lantern30.ledFans; row.platformHits = sim.lantern30.platformHits; row.culled = sim.lantern30.culled; row.lobs = sim.lantern30.lobs; }
        return row;
    }

    static string LanternLine30(LanternRow30 r, double dt) =>
        $"{(r.killed ? "KILLED at " + (r.steps * dt).ToString("0.0") + " s" : "NOT killed in " + (r.steps * dt).ToString("0") + " s (hp left " + r.hpEnd.ToString("0.00") + ")")}; deaths {r.deaths}, rides {r.rides}, full hits {r.fullHits}, light hits {r.lightHits}, platform hits {r.platformHits} (land 0), sweeps {r.sweeps} ({r.sweepTicks} burn ticks, {r.sweepDamage:0} dmg, {r.sweepDodges} dodged), led fans {r.ledFans}, lobs {r.lobs}, boss damage taken {r.hurtBoss:0}, rain decals out to {r.rainMaxR:0.0} m, orbs culled at the ring {r.culled}, floor dmg {r.floorDamage:0}, lines {r.lines} ({r.tellLines} tell / {r.coreLines} core){(r.firstLines.Length > 0 ? ": " + r.firstLines : "")}";

    static int Lantern30Command(int seeds, int first, bool dbg)
    {
        GameConfig cfgOff = Cfg30(), cfgOn = Cfg30("lantern30");
        double dt = cfgOn.Dt;
        var fails = new List<string>();
        // RED: the row off: the NIGHT29 courtyard (no ring, no sweep, no lead, no lines), the NIGHT29 rider
        {
            LanternRow30 r = LanternRun30(cfgOff, (uint)first, 240 * 120, 0, false, dbg);
            Console.WriteLine($"  RED row OFF (n30.lantern30 false; NIGHT29 rider on the courtyard) seed {first}: {LanternLine30(r, dt)}");
            if (r.ring || r.sweeps != 0 || r.lines != 0 || r.ledFans != 0) fails.Add("the row off changed the NIGHT29 fight");
        }
        var riders = new List<double>();
        int sweepsSeen = 0, lines = 0, maxTell = 0, maxCore = 0;
        for (uint sd = (uint)first; sd < (uint)(first + seeds); sd++)
        {
            LanternRow30 r = LanternRun30(cfgOn, sd, 240 * 120, 0, true, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: BossRoomBegin failed"); continue; }
            Console.WriteLine($"  GREEN rider seed {sd}: {LanternLine30(r, dt)}");
            if (r.killed) riders.Add(r.steps * dt); else fails.Add($"rider seed {sd} not killed in 240 s");
            sweepsSeen += r.sweeps; lines += r.lines; maxTell = Math.Max(maxTell, r.tellLines); maxCore = Math.Max(maxCore, r.coreLines);
        }
        double med = Median30(riders), fastest = riders.Count > 0 ? Median30(new List<double> { riders.Count > 0 ? riders[0] : 30 }) : 30;
        if (riders.Count > 0) { var s = new List<double>(riders); s.Sort(); fastest = s[0]; }
        fastest = Math.Max(fastest, med); // round 1's Player check: 3x the MEDIAN rider (the harness's fastest hid a floor camper at 143 s against a 66.6 s Player rider)
        int cap = (int)(Math.Max(fastest, 30) * 3.0 / dt) + 1;
        // the floor camper: at least 3x the fastest rider (or not killed inside 3x)
        LanternRow30 fc = LanternRun30(cfgOn, (uint)first, cap, 1, true, dbg);
        Console.WriteLine($"  GREEN floor camper (immortal, cap {cap * dt:0} s = 3x the fastest rider {fastest:0} s): {LanternLine30(fc, dt)}");
        if (fc.killed && fc.steps * dt < 3 * fastest) fails.Add($"the floor camper killed at {fc.steps * dt:0} s, under 3x the fastest rider");
        if (fc.fullHits > 0) fails.Add("the floor camper landed a full hit");
        // the platform camper: never (every hit lands 0)
        LanternRow30 pc = LanternRun30(cfgOn, (uint)first, 180 * 120, 2, true, dbg);
        Console.WriteLine($"  GREEN platform camper (immortal, 180 s): {LanternLine30(pc, dt)}");
        if (pc.killed || pc.hpEnd < 0.999) fails.Add($"the platform camper did damage (hp left {pc.hpEnd:0.00})");
        if (pc.platformHits == 0) fails.Add("the platform camper never fired from the platform (the test shows nothing)");
        if (pc.lobs == 0 || pc.hurtBoss <= 0) fails.Add($"the platform camper was never lobbed at and hurt (lobs {pc.lobs}, damage {pc.hurtBoss:0})");
        double ringIn = cfgOn.Num("design.n30.lantern30.arena.apothem") - cfgOn.Num("design.n30.lantern30.rainInset") + 1e-6;
        foreach (LanternRow30 rr in new[] { fc, pc }) if (rr.rainMaxR > ringIn) fails.Add($"a rain orb fell {rr.rainMaxR:0.0} m from the centre (ring inside {ringIn:0.0} m)");
        if (sweepsSeen == 0) fails.Add("no sweep in any rider fight");
        if (maxTell > 2 || maxCore > 2) fails.Add($"a teaching line showed more than twice in a fight (tell {maxTell}, core {maxCore})");
        if (lines == 0) fails.Add("no teaching line");
        if (riders.Count > 0 && (med < 45 || med > 75)) fails.Add($"the riders' median {med:0} s is outside 45-75 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss30 lantern {(fails.Count == 0 ? "PASS" : "FAIL")}: rider median {med:0} s over {riders.Count} kills (band 45-75 s); floor camper {(fc.killed ? (fc.steps * dt).ToString("0") + " s" : "not killed in " + (cap * dt).ToString("0") + " s")} (needs 3x = {3 * fastest:0} s); platform camper hp left {pc.hpEnd:0.00} (needs 1.00)");
        return fails.Count == 0 ? 0 : 1;
    }
    // --- boss30 wyrm -------------------------------------------------------------------------------------------------------------------------

    sealed class WyrmRow30
    {
        public int steps, charges, windowSteps, swings, swingsInWindow, gauntletHits, staggers, staggerEvents, hitstops, recoils, deaths, chargeAfter;
        public double hitstopSeconds, bonus, hpLost, recoilSecondsMin = 99, recoilSecondsMax;
        public string after = "";
    }

    /// <summary>The PUNCHER: level 3's worm (n26 level3: the proximity aggro) and a bot that walks up to provoke a charge, faces the head, and swings the PULSE
    /// gauntlet when the charging head's box comes within the swing's reach (the same moment with the row on or off). Immortal; `secs` of fight.</summary>
    static WyrmRow30 WyrmRun30(GameConfig cfg, uint seed, double secs, bool on, bool dbg)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n24Override = new N24Blocks { wyrm = true };
        sim.n26Override = new N26Blocks { level3 = true };
        sim.n29Override = new N29Blocks { boss1 = true };
        sim.n30Override = new N30Blocks { wyrm30 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new WyrmRow30();
        if (!sim.Wyrm24Begin()) { row.steps = -1; return row; }
        sim.Respawn(sim.Wyrm24Spawn, 0);
        Wyrm w = sim.wyrm24;
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        int gi = sim.loadout.owned.IndexOf("gauntlet");
        sim.events.On<Skygrind.Game.SurvivalSim.Wyrm30Event>("wyrmStagger", e => { row.staggerEvents++; row.bonus += e.bonus; });
        sim.events.On<string>("hitstop", c => { if (c == "wyrmStagger") { row.hitstops++; row.hitstopSeconds = sim.hitstopLeft; } });
        sim.events.On<HitEvent>("hit", e => { if (e.weapon == "gauntlet" && e.enemy == w.body.id) row.gauntletHits++; });
        var si = (ScriptedInput)sim.input;
        double reach = cfg.Num("weapons.gauntlet.range");
        double hp0 = 0;
        bool wasCharging = false, wasRecoil = false, swungThisCharge = false, watchAfter = false;
        int recoilStart = 0;
        int cap = (int)(secs / dt);
        for (int step = 0; step < cap && w.Alive; step++)
        {
            if (step == 1) hp0 = w.body.hp;
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 head = w.boxes[0].c;
            Vec3 d = head - eye;
            double dist = d.Length;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            bool charging = w.Charging30;
            if (charging && !wasCharging) { row.charges++; swungThisCharge = false; }
            wasCharging = charging;
            var mv = new InputFrame { yaw = wy, pitch = wp };
            if (!charging && !w.Recoiling30 && dist > 9) { mv.moveZ = 1; } // walk up to the head (inside the aggro radius 14 m: the worm charges)
            bool swing = false;
            if (charging && !swungThisCharge && w.boxes[0].Touches(eye, reach))
            {
                swing = true;
                swungThisCharge = true;
                row.swings++;
                if (sim.Wyrm30Punchable) row.swingsInWindow++;
            }
            if (sim.Wyrm30Punchable) row.windowSteps++;
            si.SetLook(wy, wp);
            si.Set(mv, new CombatInput { weapon = sim.loadout.Weapon != "gauntlet" ? gi : -1, fire = swing ? new Skygrind.Core.Button { held = true, pressed = true } : default });
            sim.Step();
            row.steps++;
            bool rec = w.Recoiling30;
            if (rec && !wasRecoil) { row.recoils++; recoilStart = step; }
            if (!rec && wasRecoil)
            {
                double rs = (step - recoilStart) * dt;
                row.recoilSecondsMin = Math.Min(row.recoilSecondsMin, rs);
                row.recoilSecondsMax = Math.Max(row.recoilSecondsMax, rs);
                watchAfter = true;
            }
            wasRecoil = rec;
            if (watchAfter)
            {
                watchAfter = false;
                row.after += (row.after.Length > 0 ? ", " : "") + (w.PatternName ?? "?");
                if (w.AggroDiving || w.Lunging) row.chargeAfter++;
            }
            sim.health.hp = sim.health.max;
            sim.hitstopLeft = 0; // the frame loop (SurvivalGame) counts hitstop down; headless, each step clears it so the next request is seen
            if (dbg && step % 600 == 0) Console.WriteLine($"    t={step * dt:0}s {w.phase} pat {w.PatternName} hp {w.HpFrac:0.00} dist {dist:0.0} charging {charging} contact {sim.Wyrm30Contact:0.00}");
        }
        row.staggers = sim.wyrm30Staggers;
        row.hpLost = hp0 - w.body.hp;
        return row;
    }

    static int Wyrm30Command(int seeds, int first, bool dbg)
    {
        GameConfig cfgOff = Cfg30(), cfgOn = Cfg30("wyrm30");
        double dt = cfgOn.Dt, secs = 90;
        var fails = new List<string>();
        int n = Math.Min(seeds, 3);
        double want = cfgOn.Num("design.n30.wyrm30.hitstopSteps") * dt, rec = cfgOn.Num("design.n30.wyrm30.recoilSeconds");
        int stOn = 0, stOff = 0;
        for (uint sd = (uint)first; sd < (uint)(first + n); sd++)
        {
            WyrmRow30 r0 = WyrmRun30(cfgOff, sd, secs, false, dbg);
            WyrmRow30 r1 = WyrmRun30(cfgOn, sd, secs, true, dbg);
            if (r0.steps < 0 || r1.steps < 0) { fails.Add($"seed {sd}: Wyrm24Begin failed"); continue; }
            Console.WriteLine($"  RED row OFF seed {sd} ({secs:0} s, the puncher): charges {r0.charges}, swings at the charging head {r0.swings}, gauntlet hits {r0.gauntletHits}, staggers {r0.staggers}, wyrmStagger events {r0.staggerEvents}, recoils {r0.recoils}, worm hp lost {r0.hpLost:0}");
            Console.WriteLine($"  GREEN row ON seed {sd}: charges {r1.charges}, window open {r1.windowSteps * dt:0.00} s, swings {r1.swings} ({r1.swingsInWindow} in the window), gauntlet hits {r1.gauntletHits}, staggers {r1.staggers}, events {r1.staggerEvents}, hitstops {r1.hitstops} ({r1.hitstopSeconds:0.000} s, want {want:0.000}), bonus {r1.bonus:0}, recoils {r1.recoils} lasting {(r1.recoils > 0 ? r1.recoilSecondsMin.ToString("0.00") + "-" + r1.recoilSecondsMax.ToString("0.00") : "-")} s (want {rec:0.00}), then [{r1.after}] (a charge right after: {r1.chargeAfter}), worm hp lost {r1.hpLost:0}");
            stOn += r1.staggers; stOff += r0.staggers + r0.staggerEvents;
            if (r1.hitstops > 0 && Math.Abs(r1.hitstopSeconds - want) > 1e-9) fails.Add($"seed {sd}: hitstop {r1.hitstopSeconds} s, not {want}");
            if (r1.chargeAfter > 0) fails.Add($"seed {sd}: a charge right after a stagger");
            if (r1.recoils > 0 && (r1.recoilSecondsMin < rec - 0.1 || r1.recoilSecondsMax > rec + 0.1)) fails.Add($"seed {sd}: a recoil outside {rec} s +- 0.1");
            if (r1.staggers != r1.staggerEvents || r1.staggers != r1.hitstops) fails.Add($"seed {sd}: staggers / events / hitstops disagree");
        }
        // item 11 (D-5): the worm's kill time with wyrm30 on, the composed sim's human-lag bot (N24's WyrmRig / WyrmBot: the revolver, never the gauntlet: the stagger's bonus is not in it),
        // level 3's worm (n26 level3: the aggro charges), NIGHT29's HP cut (n29 boss1); band 3:00-4:00
        {
            var kt = new List<double>();
            int dsum = 0;
            for (uint sd = (uint)first + 2; sd < (uint)(first + 2 + Math.Min(seeds, 3)); sd++)
            {
                SurvivalSim sim = SurvivalSim.Headless(cfgOn, Map());
                sim.noLab = sim.noRecords = true;
                sim.n24Override = new N24Blocks { wyrm = true };
                sim.n26Override = new N26Blocks { level3 = true };
                sim.n29Override = new N29Blocks { boss1 = true };
                sim.n30Override = new N30Blocks { wyrm30 = true };
                sim.setup.mode = RunSetup.Survival;
                sim.StartRun(sd, null, null);
                sim.husks.frozen = true;
                if (!sim.Wyrm24Begin()) { fails.Add("Wyrm24Begin failed (kill time)"); break; }
                sim.Respawn(sim.Wyrm24Spawn, 0);
                var rig = new WyrmRig(sim);
                var bot = new WyrmBot(true);
                while (sim.wyrm24.Alive && rig.step < 12 * 60 * 120)
                {
                    InputFrame f = bot.Next(rig, out bool fire);
                    rig.Step(f, fire);
                    sim.hitstopLeft = 0;
                }
                double s = rig.step * dt;
                dsum += rig.deaths;
                Console.WriteLine($"  GREEN kill (wyrm30 on, level 3's worm, the human-lag WyrmBot) seed {sd}: {(!sim.wyrm24.Alive ? "KILLED at " + (int)(s / 60) + ":" + (s % 60).ToString("00") : "NOT killed in 12:00")}; bot deaths {rig.deaths}, staggers {sim.wyrm30Staggers}");
                if (!sim.wyrm24.Alive) kt.Add(s);
            }
            double km = Median30(kt);
            Console.WriteLine($"  kill summary: {kt.Count} kills, median {(int)(km / 60)}:{km % 60:00} (band 3:00-4:00), bot deaths {dsum}");
            if (kt.Count == 0 || km < 180 || km > 240) fails.Add($"the worm's lag-bot median {(int)(km / 60)}:{km % 60:00} is outside 3:00-4:00");
        }
        if (stOff != 0) fails.Add("a stagger with the row off");
        if (stOn == 0) fails.Add("no stagger with the row on (the puncher never landed one)");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss30 wyrm {(fails.Count == 0 ? "PASS" : "FAIL")}: staggers row off {stOff}, row on {stOn} over {n} seeds x {secs:0} s");
        return fails.Count == 0 ? 0 : 1;
    }
    // --- boss30 loom ---------------------------------------------------------------------------------------------------------------------------

    /// <summary>THE LOOM's first pass, as unit lines on a standalone rig (the real Mover, a BossKit, the Loom at s8's deck from the plan's loomLegs): the damage rule (light off the line,
    /// full within 3 m of a joint or hanging on that leg's line, light on the body), the joints' glow flags, the CUT (tell 1.0 s on a hanging player, then the rope released and a drop;
    /// a player who lets go during the tell is not cut), and the RED: with design.n30.span off the composed sim's BossRoomBegin("loom") starts nothing. No grapple-and-nail pilot yet.</summary>
    static int Loom30Command(bool dbg)
    {
        GameConfig cfg = Cfg30("span");
        double dt = cfg.Dt;
        var fails = new List<string>();
        void Row(string name, bool ok, string detail) { Console.WriteLine($"  {(ok ? "PASS" : "FAIL")} {name}: {detail}"); if (!ok) fails.Add(name); }
        // RED: the block off
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg30(), Map());
            sim.noLab = sim.noRecords = true;
            sim.n30Override = N30Blocks.Off;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(1, null, null);
            bool began = sim.BossRoomBegin("loom", out _, out _);
            Console.WriteLine($"  RED span OFF: BossRoomBegin(\"loom\") {(began ? "STARTED (wrong)" : "starts nothing")}, BossRoomSystem(\"loom\") {(sim.BossRoomSystem("loom") == null ? "null" : "set")}");
            if (began) fails.Add("the Loom started with the span block off");
        }
        // the rig: s8's deck (the plan's loomLegs), a flat floor, the real Mover
        var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine(Root, "..", "Design", "levels", "n30", "span.plan.json"))));
        Vec3[] legs = null;
        foreach (object ro in PortData.Arr(plan["rooms"]))
        {
            var r = PortData.Obj(ro);
            if (r.ContainsKey("loomLegs")) { var a = PortData.Arr(r["loomLegs"]); legs = new Vec3[4]; for (int i = 0; i < 4; i++) legs[i] = PortData.V3(a[i]); }
        }
        Console.WriteLine($"  rig: loomLegs {(legs != null ? "read from span.plan.json" : "NOT in the plan (centre +- legHalf)")}");
        var events = new EventBus();
        var w = new TriangleWorld();
        Vec3 deck = legs != null ? new Vec3((legs[0].x + legs[2].x) * 0.5, legs[0].y, (legs[0].z + legs[2].z) * 0.5) : new Vec3(0, 0, -218);
        w.AddBox(new Vec3(deck.x - 20, deck.y - 1, deck.z - 20), new Vec3(deck.x + 20, deck.y, deck.z + 20));
        var mover = new Mover(cfg.movement, w);
        mover.Reset(deck.x, deck.y + 0.05, deck.z);
        var health = new Health(100, events, "player");
        var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
        var kit = new BossKit(cfg, events, null, target);
        int ids = 0;
        var loom = new Loom30(cfg, Loom30Params.From(cfg), events, target, () => ++ids);
        loom.playerHanging = () => mover.rope.phase == RopePhase.Attached;
        loom.ropeAnchor = () => mover.rope.anchor;
        loom.cutLine = drop => mover.Launch(-drop, mover.velocity.x * 0.5, mover.velocity.z * 0.5);
        loom.Place(deck, legs);
        kit.bosses["loom"] = loom;
        kit.Start("loom");
        int steps = 0;
        void Run(int n) { for (int i = 0; i < n && loom.Alive; i++) { loom.Step(dt); kit.Step(dt); steps++; } }
        Run(MovementMath.StepsFor(loom.p.introHold + 2.5, dt));
        Row("intro over", loom.phase == "p1", $"phase {loom.phase} after {steps * dt:0.0} s");
        Console.WriteLine($"  the joints (world): {string.Join(" ", System.Linq.Enumerable.Select(loom.joints, j => $"({j.x:0.0},{j.y:0.0},{j.z:0.0})"))}; body at ({loom.box.c.x:0.0},{loom.box.c.y:0.0},{loom.box.c.z:0.0})");
        double Shot(int joint, Vec3 from)
        {
            Vec3 to = joint >= 0 ? loom.joints[joint] : loom.box.c, d = Vec3.Normalize(to - from);
            double t = loom.Raycast(from, d);
            if (t < 0) return -1;
            return loom.Filter(100, "nailgun", out bool blocked) * (blocked ? 0 : 1);
        }
        Vec3 deckEye = new Vec3(deck.x, deck.y + 1.6, deck.z);
        double off = Shot(0, deckEye);
        Row("joint off the line", Math.Abs(off - 100 * loom.p.lightScale) < 1e-9, $"a 100 hit on joint 0 from the deck's middle (not hanging, {Vec3.DistanceTo(deckEye, loom.joints[0]):0.0} m away) lands {off:0.0} (light {loom.p.lightScale})");
        double body = Shot(-1, deckEye);
        Row("body", Math.Abs(body - 100 * loom.p.lightScale) < 1e-9, $"a 100 hit on the body lands {body:0.0}");
        // from the deck's corner (the grip) the hip under the edge is out of fullReach: a camper on the deck lands light only
        Vec3 g0 = loom.grips[0];
        mover.Reset(g0.x, g0.y + 0.05, g0.z);
        double corner = Shot(1, new Vec3(g0.x, g0.y + 1.6, g0.z));
        Row("deck corner camper", Math.Abs(corner - 100 * loom.p.lightScale) < 1e-9, $"standing on the grip, {Vec3.DistanceTo(new Vec3(g0.x, g0.y + 1.0, g0.z), loom.joints[1]):0.0} m from the hip (fullReach {loom.p.fullReach}): a hit lands {corner:0.0}");
        // within 3 m of joint 1 (the hip): hanging next to it
        Vec3 j1 = loom.joints[1];
        mover.Reset(j1.x, j1.y - 1.0 - 0.5, j1.z);
        double near = Shot(1, new Vec3(j1.x + 0.5, j1.y + 0.5, j1.z + 0.5));
        Row("within fullReach", Math.Abs(near - 100) < 1e-9, $"standing {Vec3.DistanceTo(new Vec3(mover.position.x, mover.position.y + 1, mover.position.z), j1):0.0} m from joint 1: a hit lands {near:0.0}");
        // hanging on leg 0's line: the rope's anchor on joint 0
        mover.Reset(deck.x, deck.y + 6, deck.z);
        mover.rope.phase = RopePhase.Attached;
        mover.rope.anchor = loom.joints[0];
        double hang0 = Shot(1, deckEye), hang2 = Shot(2, deckEye);
        loom.Step(dt); kit.Step(dt);
        Row("hanging on leg 0", Math.Abs(hang0 - 100) < 1e-9 && Math.Abs(hang2 - 100 * loom.p.lightScale) < 1e-9, $"anchor on joint 0: a hit on joint 1 (leg 0) lands {hang0:0.0}, on joint 2 (leg 1) {hang2:0.0}; glow flags {string.Join("", System.Linq.Enumerable.Select(loom.jointFull, b => b ? "1" : "0"))} (legs 0..3, knee/hip)");
        Row("glow follows the rule", loom.jointFull[0] && loom.jointFull[1] && !loom.jointFull[2] && !loom.jointFull[6], "joints 0 and 1 glow, 2 and 6 do not");
        // CUT: hang on and wait for the tell, then the cut
        var cutLog = new List<string>();
        int tellAt = -1, cutAt = -1;
        events.On<LoomCut30>("loomCut", e => { cutLog.Add($"{e.stage}@{steps * dt:0.00}"); if (e.stage == "tell") tellAt = steps; else cutAt = steps; });
        for (int i = 0; i < 12 * 120 && cutAt < 0; i++) { mover.rope.phase = cutAt < 0 && mover.rope.phase == RopePhase.None && tellAt < 0 ? RopePhase.Attached : mover.rope.phase; loom.Step(dt); kit.Step(dt); steps++; }
        double tellLen = tellAt >= 0 && cutAt >= 0 ? (cutAt - tellAt) * dt : -1;
        Row("CUT", cutAt >= 0 && Math.Abs(tellLen - loom.p.cutTellSeconds) < 0.02 && mover.rope.phase == RopePhase.None && mover.velocity.y < 0, $"tell -> cut {tellLen:0.00} s (want {loom.p.cutTellSeconds}); after it the rope {mover.rope.phase}, vy {mover.velocity.y:0.0} m/s; events {string.Join(", ", cutLog)}");
        // the dodge: hang on again, let go during the tell
        int dodged0 = loom.cutsDodged, cuts0 = loom.cuts;
        mover.rope.phase = RopePhase.Attached;
        mover.rope.anchor = loom.joints[4];
        bool released = false;
        for (int i = 0; i < 12 * 120 && loom.cutsDodged == dodged0 && loom.cuts == cuts0; i++)
        {
            if (loom.CutTelling && loom.CutTellProgress > 0.5 && !released) { mover.rope.phase = RopePhase.None; released = true; }
            loom.Step(dt); kit.Step(dt); steps++;
        }
        Row("let go in the tell", loom.cutsDodged == dodged0 + 1 && loom.cuts == cuts0, $"released half way through the tell: dodged {loom.cutsDodged - dodged0}, cut {loom.cuts - cuts0}");
        // not hanging: no tell at all for 12 s
        int tells0 = loom.cutTells;
        mover.rope.phase = RopePhase.None;
        Run(12 * 120);
        Row("no line, no cut", loom.cutTells == tells0, $"12 s on the deck: {loom.cutTells - tells0} tells");
        // SWEEP: a player standing on the deck is swept (tell 1.2 s, then 20 damage and a knock); one who jumps in the tell is not; a player on a rope is never swept
        loom.playerGrounded = () => mover.grounded;
        mover.Reset(deck.x + 2, deck.y + 0.05, deck.z + 2);
        for (int i = 0; i < 30; i++) mover.Step(new InputFrame(), dt);
        var sweepLog = new List<string>();
        int sTell = -1, sLive = -1;
        events.On<LoomCut30>("loomSweep", e => { sweepLog.Add($"{e.stage}@{steps * dt:0.00}"); if (e.stage == "tell") sTell = steps; else if (e.stage == "live") sLive = steps; });
        double hp0 = health.hp;
        int hits0 = loom.sweepHits;
        for (int i = 0; i < 20 * 120 && loom.sweepHits == hits0 && loom.sweepsDodged == 0; i++) { mover.Step(new InputFrame(), dt); loom.Step(dt); kit.Step(dt); steps++; }
        double sTellLen = sTell >= 0 && sLive >= 0 ? (sLive - sTell) * dt : -1;
        Row("SWEEP a stander", loom.sweepHits == hits0 + 1 && Math.Abs(sTellLen - loom.p.sweepTellSeconds) < 0.02 && health.hp < hp0, $"tell -> sweep {sTellLen:0.00} s (want {loom.p.sweepTellSeconds}); hp {hp0:0} -> {health.hp:0}, knocked to vy {mover.velocity.y:0.0}; events {string.Join(", ", sweepLog)}");
        // the jumper: back on the deck once the stander's sweep has run out, a jump when the bar is close
        for (int i = 0; i < 5 * 120 && (loom.SweepLive || loom.SweepTelling); i++) { loom.Step(dt); kit.Step(dt); steps++; }
        mover.Reset(deck.x - 2, deck.y + 0.05, deck.z - 2);
        mover.velocity = default;
        for (int i = 0; i < 30; i++) mover.Step(new InputFrame(), dt);
        int dodged1 = loom.sweepsDodged, hits1 = loom.sweepHits, jumpAt = -1;
        double feetMax = 0;
        bool jumpGrounded = false;
        string jumpState = "", hitInfo = "";
        for (int i = 0; i < 20 * 120 && loom.sweepsDodged == dodged1 && loom.sweepHits == hits1; i++)
        {
            // a human jumps when he sees the bar about 4 m away (the bar at ~17 m/s: 0.25 s)
            bool jump = false;
            if (loom.SweepLive && jumpAt < 0 && loom.SweepLeg >= 0)
            {
                Vec3 ga = loom.grips[loom.SweepLeg], gb = loom.grips[(loom.SweepLeg + 2) % 4];
                double ax = gb.x - ga.x, az = gb.z - ga.z, al = JsMath.Hypot(ax, az);
                double along = ((mover.position.x - ga.x) * ax + (mover.position.z - ga.z) * az) / al, gap = along - loom.SweepBar * al;
                jump = gap > 0 && gap <= 6.0;
            }
            if (jump) { jumpAt = steps; jumpGrounded = mover.grounded; jumpState = mover.state.ToString(); }
            mover.Step(new InputFrame { jump = new Skygrind.Core.Button { held = jump || (jumpAt >= 0 && steps - jumpAt < 20), pressed = jump } }, dt);
            if (jumpAt >= 0) feetMax = Math.Max(feetMax, mover.position.y - deck.y);
            int hb = loom.sweepHits;
            loom.Step(dt); kit.Step(dt); steps++;
            if (loom.sweepHits != hb) hitInfo = $"hit {(jumpAt >= 0 ? (steps - jumpAt) * dt : -1):0.00} s after the press, feet {mover.position.y - deck.y:0.00} m";
            continue;
            loom.Step(dt); kit.Step(dt); steps++;
        }
        Row("SWEEP jumped", loom.sweepsDodged == dodged1 + 1 && loom.sweepHits == hits1, $"a jump when the bar is 6 m away (0.35 s): jumped {(jumpAt >= 0 ? "yes" : "NO")}, dodged {loom.sweepsDodged - dodged1}, hit {loom.sweepHits - hits1}, feet {mover.position.y - deck.y:0.00} m at the end, highest {feetMax:0.00} m, at the press grounded {jumpGrounded} state {jumpState} {hitInfo}");
        // on a rope: no sweep tell
        int sw0 = loom.sweeps;
        mover.rope.phase = RopePhase.Attached;
        mover.rope.anchor = loom.joints[2];
        for (int i = 0; i < 12 * 120; i++) { loom.Step(dt); kit.Step(dt); steps++; if (loom.CutTelling) mover.rope.phase = RopePhase.Attached; }
        Row("no sweep on a rope", loom.sweeps == sw0, $"12 s on a rope: {loom.sweeps - sw0} sweeps (cuts meanwhile {loom.cuts})");
        mover.rope.phase = RopePhase.None;
        // the kill time: the LAG BOT (a scripted model on the same rig: no rope physics) over 6 seeds; band 90-150 s (D-5)
        var kills = new List<double>();
        for (uint sd = 1; sd <= 6; sd++)
        {
            LoomBotRow r = LoomBotRun(cfg, legs, deck, sd);
            Console.WriteLine($"  GREEN lag bot seed {sd}: {(r.killed ? "KILLED at " + r.seconds.ToString("0.0") + " s" : "NOT killed in " + r.seconds.ToString("0") + " s (hp " + r.hpEnd.ToString("0.00") + ")")}; hangs {r.hangs}, cut tells {r.tells} (let go {r.dodged}, cut {r.cuts}), nails {r.nails} (full {r.full}, light {r.light}), hanging {r.hangShare * 100:0} % of the fight");
            if (r.killed) kills.Add(r.seconds); else fails.Add($"lag bot seed {sd} not killed");
        }
        double kmed = Median30(kills);
        if (kills.Count > 0 && (kmed < 90 || kmed > 150)) fails.Add($"the lag bot's median {kmed:0} s is outside 90-150 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss30 loom {(fails.Count == 0 ? "PASS" : "FAIL")}: the damage rule, the glow, the CUT (tell {tellLen:0.00} s) and its dodge; lag bot median {kmed:0} s over {kills.Count} kills (band 90-150 s; a scripted model: hang windows, a 0.35 s reaction to the CUT tell, nailgun heat; no rope physics)");
        return fails.Count == 0 ? 0 : 1;
    }

    sealed class LoomBotRow
    {
        public bool killed;
        public double seconds, hpEnd, hangShare;
        public int hangs, tells, dodged, cuts, nails, full, light;
    }

    /// <summary>THE LOOM's lag bot, a scripted model on the standalone rig (the real Loom30 rule and CUT, the real nailgun numbers; no rope physics): it travels to a leg (swingSeconds),
    /// hangs on that leg's knee (the rope's anchor on it, the body 3 m under) and streams nails at the leg's joints (a seeded hand: hitChance of the nails land on the aimed joint, the rest
    /// on nothing), sees the CUT tell after reactSeconds and lets go (missChance of the tells it misses: cut, a recoverSeconds drop), then regrabs the next leg; off a line it fires at
    /// the nearest joint from the deck (light). The nailgun's heat (heatTime of fire, then lockTime) applies. Bot numbers shape one opponent, not the rule.</summary>
    static LoomBotRow LoomBotRun(GameConfig cfg, Vec3[] legs, Vec3 deck, uint seed, bool n31 = false)
    {
        double dt = cfg.Dt;
        const double swingSeconds = 2.0, reactSeconds = 0.35, hitChance = 0.6, missChance = 0.15, recoverSeconds = 3.5, deckSeconds = 1.5;
        double rate = cfg.Num("weapons.nailgun.rate"), dmg = cfg.Num("weapons.nailgun.damage"), heat = cfg.Num("weapons.nailgun.heatTime"), lockT = cfg.Num("weapons.nailgun.lockTime");
        var events = new EventBus();
        var w = new TriangleWorld();
        w.AddBox(new Vec3(deck.x - 20, deck.y - 1, deck.z - 20), new Vec3(deck.x + 20, deck.y, deck.z + 20));
        var mover = new Mover(cfg.movement, w);
        mover.Reset(deck.x, deck.y + 0.05, deck.z);
        var health = new Health(100, events, "player");
        var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => { } };
        var kit = new BossKit(cfg, events, null, target);
        int ids = 0;
        var loom = new Loom30(cfg, Loom30Params.From(cfg), events, target, () => ++ids);
        var row = new LoomBotRow();
        bool cutNow = false;
        loom.playerHanging = () => mover.rope.phase == RopePhase.Attached;
        loom.ropeAnchor = () => mover.rope.anchor;
        loom.cutLine = drop => { mover.rope.phase = RopePhase.None; cutNow = true; };
        loom.Place(deck, legs);
        kit.bosses["loom"] = loom;
        kit.Start("loom");
        Loom31 l31 = null; // NIGHT31 B (camp31 loom31): the NIGHT31 rules on the same rig (reach, raised hips, the shake, the drop, the break)
        if (n31 && cfg.Has("design.n31.loom31.fullReach")) { l31 = new Loom31(cfg, Loom31Params.From(cfg), events, target); l31.Reset(); l31.Hook(loom); }
        var hand = new Mulberry32(seed * 7919u + 13u);
        int state = 0, timer = MovementMath.StepsFor(swingSeconds, dt), leg = (int)(seed % 4), react = -1, hangSteps = 0, playSteps = 0;
        double fireAcc = 0, heatLeft = heat, lockLeft = 0;
        int step = 0, cap = 400 * 120;
        while (loom.Alive && step < cap)
        {
            loom.Step(dt); kit.Step(dt); step++;
            if (loom.phase != "p1") continue;
            if (l31 != null && loom.phase == "p1") l31.Step(loom, mover.rope.phase == RopePhase.Attached, state == 2, dt);
            playSteps++;
            // the bot's state: 0 travelling to a leg, 1 hanging on it, 2 on the deck (after letting go or a cut)
            if (state == 0 && --timer <= 0)
            {
                state = 1; row.hangs++;
                Vec3 k = loom.joints[2 * leg];
                mover.Reset(k.x, k.y - 3.5, k.z);
                mover.rope.phase = RopePhase.Attached;
                mover.rope.anchor = k;
                react = -1;
            }
            else if (state == 1)
            {
                hangSteps++;
                if ((loom.CutTelling || (l31 != null && l31.ShakeTelling)) && react < 0) { row.tells++; react = hand.Next() < missChance ? int.MaxValue : MovementMath.StepsFor(reactSeconds, dt); }
                if (react >= 0 && react != int.MaxValue && --react == 0) { mover.rope.phase = RopePhase.None; row.dodged++; state = 2; timer = MovementMath.StepsFor(deckSeconds, dt); leg = (leg + 1) % 4; mover.Reset(deck.x, deck.y + 0.05, deck.z); }
            }
            else if (state == 2 && --timer <= 0) { state = 0; timer = MovementMath.StepsFor(swingSeconds, dt); }
            if (cutNow) { cutNow = false; row.cuts++; state = 2; timer = MovementMath.StepsFor(recoverSeconds, dt); leg = (leg + 1) % 4; mover.Reset(deck.x, deck.y + 0.05, deck.z); }
            // the nailgun: heatTime of fire, then lockTime locked
            if (lockLeft > 0) { lockLeft -= dt; if (lockLeft <= 0) heatLeft = heat; continue; }
            if (state == 0) continue; // the swing: both hands busy
            heatLeft -= dt;
            if (heatLeft <= 0) { lockLeft = lockT; continue; }
            fireAcc += rate * dt;
            while (fireAcc >= 1)
            {
                fireAcc -= 1;
                if (hand.Next() >= hitChance) continue;
                Vec3 me = mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
                int j = state == 1 ? 2 * leg + (hand.Next() < 0.5 ? 0 : 1) : NearestJoint(loom, eye);
                Vec3 d = Vec3.Normalize(loom.joints[j] - eye);
                if (loom.Raycast(eye, d) < 0) continue;
                int full0 = loom.fullHits;
                double landed = loom.Filter(dmg, "nailgun", out bool blocked);
                if (blocked) continue;
                loom.Apply(landed);
                row.nails++;
                if (loom.fullHits > full0) row.full++; else row.light++;
            }
        }
        row.killed = !loom.Alive || loom.phase == "dying";
        row.seconds = playSteps * dt;
        row.hpEnd = loom.HpFrac;
        row.hangShare = playSteps > 0 ? (double)hangSteps / playSteps : 0;
        if (l31 != null) campLoom31Line = $"shake tells {l31.shakeTells} (thrown {l31.shakes}, let go {l31.shakesDodged}), drops {l31.drops} ({l31.dropHits} hit), break {l31.breakState}, lava hits {l31.lavaHits}";
        return row;
    }

    static int NearestJoint(Loom30 l, Vec3 eye)
    {
        int best = 0;
        double bd = 1e9;
        for (int i = 0; i < l.joints.Length; i++) { double d = Vec3.DistanceTo(eye, l.joints[i]); if (d < bd) { bd = d; best = i; } }
        return best;
    }

    // --- boss30 glacier -------------------------------------------------------------------------------------------------------------------------

    sealed class GlacierRow30
    {
        public int steps, deaths, volleys, shardHits, sweeps, sweepHits, exposedWindows, bossHits, glideSteps, dashAdds;
        public bool killed, big;
        public double hpEnd, longestQuiet, longestOut, rinkW, rinkL, gapMax;
        public string bankAt = "";
    }

    /// <summary>mode 0: the chaser lag bot (GlacierPilot29); 1: the BANK CAMPER (GG's safe zone: stands near the top of the outer bank, walks back when it slides, pistol at the boss on sight;
    /// immortal). The quiet time is the longest stretch between two boss hits on the player; the out-of-reach time is Glacier30.OutOfReach's longest stretch.</summary>
    static GlacierRow30 GlacierRun30(GameConfig cfg, uint seed, int capSteps, int mode, bool on, bool dbg, bool glide = false)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { pulse27 = true };
        sim.n29Override = new N29Blocks { boss2 = true };
        sim.n30Override = new N30Blocks { glacier30 = on, glide = glide };
        sim.n26Override = new N26Blocks { surfaces = true, level2 = true };
        sim.bosses25Override = true;
        sim.glacier28Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new GlacierRow30();
        if (!sim.Glacier28Begin()) { row.steps = -1; return row; }
        sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
        Glacier28 g = sim.glacier28;
        Rink28Arena a = sim.glacier28Rink;
        row.rinkW = 2 * (a.p.islandRadius + a.p.laneWidth + a.p.bankRun + a.p.lip);
        row.rinkL = a.p.straight + row.rinkW;
        row.big = on;
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        var si = (ScriptedInput)sim.input;
        var pilot = new GlacierPilot29 { hand = new Mulberry32(seed * 7919u + 13u) };
        int lastHit = 0, outRun = 0, quietMax = 0, outMax = 0;
        sim.events.On<HurtEvent>("hurt", e => { if (e.source == "boss") { row.bossHits++; } });
        int hitsSeen = 0;
        double yawL = sim.Glacier28SpawnYawDeg * Math.PI / 180;
        double campS = a.p.playerStartS + 20, campLat = a.latBankTop - 0.6;
        Vec3 camp = a.FloorPoint(campS, campLat);
        row.bankAt = $"s {campS:0} lat {campLat:0.0} (bank top {a.latBankTop:0.0}, lane out {a.latLaneOut:0.0}) y {camp.y - a.p.cy:0.0}";
        if (mode == 1) sim.Respawn(new Vec3(camp.x, camp.y + 0.1, camp.z), 0);
        int step = 0, playStart = -1;
        while (g.Alive && step < capSteps)
        {
            if (mode == 0)
            {
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw;
                si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                si.Set(pilot.Move, pilot.Combat);
            }
            else if (mode == 2)
            {
                // the FLEER: runs away from the boss along the lap (the other way round), sprinting, never shooting
                int flee = g.gapSigned >= 0 ? -1 : 1;
                g.rink.PointAt(g.playerS + flee * 6, 0, out double tx, out double tz, out _, out _);
                Vec3 me = sim.mover.position;
                double wx = tx - me.x, wz = tz - me.z;
                double wy = JsMath.Atan2(-wx, -wz);
                si.SetLook(wy, 0);
                si.Set(new InputFrame { yaw = wy, moveZ = 1, sprint = new Skygrind.Core.Button { held = true, pressed = step % 60 == 0 } }, new CombatInput { weapon = -1 });
            }
            else
            {
                Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = g.box.c - eye;
                double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                var mv = new InputFrame { yaw = wy, pitch = wp };
                double cx = camp.x - me.x, cz = camp.z - me.z, cl = JsMath.Hypot(cx, cz);
                if (cl > 1.0)
                {
                    Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                    mv.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
                    mv.moveX = (cx * right.x + cz * right.z) / cl;
                }
                Vec3 dn = Vec3.Normalize(d);
                double tHit = g.Raycast(eye, dn);
                bool see = tHit >= 0 && !(sim.world.RayIntersect(eye, dn, out RayHit wh) && wh.distance < tHit);
                int gun = sim.loadout.owned.IndexOf("revolver");
                si.SetLook(wy, wp);
                si.Set(mv, new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Skygrind.Core.Button { held = see, pressed = see && step % 4 == 0 } });
            }
            sim.Step();
            step++;
            if (mode != 0) sim.health.hp = sim.health.max;
            if (g.IsPlaying)
            {
                if (playStart < 0) { playStart = step; lastHit = step; }
                if (row.bossHits != hitsSeen) { hitsSeen = row.bossHits; quietMax = Math.Max(quietMax, step - lastHit); lastHit = step; }
                bool outNow = sim.glacier30 != null && on && sim.glacier30.OutOfReach;
                outRun = outNow ? outRun + 1 : 0;
                outMax = Math.Max(outMax, outRun);
                row.gapMax = Math.Max(row.gapMax, g.gap);
            }
            if (dbg && step % 1200 == 0) Console.WriteLine($"    t={step * dt:0}s {g.phase} hp {g.HpFrac:0.00} gap {g.gap:0.0} gauge {sim.glacier30?.GaugeFrac:0.00} far {sim.glacier30?.Far} hidden {sim.glacier30?.Hidden} volleys {sim.glacier30?.volleys} sweeps {sim.glacier30?.sweeps} php {sim.health.hp:0}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Glacier28Begin();
                sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            }
        }
        if (playStart >= 0) quietMax = Math.Max(quietMax, step - lastHit);
        row.steps = step;
        row.killed = !g.Alive;
        row.hpEnd = g.HpFrac;
        row.longestQuiet = quietMax * dt;
        row.longestOut = outMax * dt;
        if (sim.glacier30 != null && on) { row.volleys = sim.glacier30.volleys; row.shardHits = sim.glacier30.shardHits; row.sweeps = sim.glacier30.sweeps; row.sweepHits = sim.glacier30.sweepHits; }
        if (sim.glacier29 != null) row.exposedWindows = sim.glacier29.exposedWindows;
        if (sim.glide30 != null) { row.glideSteps = sim.glide30.glideSteps; row.dashAdds = sim.glide30.dashAdds; }
        return row;
    }

    static string GlacierLine30(GlacierRow30 r, double dt) =>
        $"{(r.killed ? "KILLED at " + (r.steps * dt).ToString("0.0") + " s" : "NOT killed in " + (r.steps * dt).ToString("0") + " s (bar " + r.hpEnd.ToString("0.00") + ")")}; rink {r.rinkW:0} x {r.rinkL:0} m; deaths {r.deaths}, boss hits on the player {r.bossHits}, longest quiet {r.longestQuiet:0.0} s, longest out of reach {r.longestOut:0.0} s, volleys {r.volleys} ({r.shardHits} shard hits), sweeps {r.sweeps} ({r.sweepHits} hits), exposed windows {r.exposedWindows}, gap max {r.gapMax:0} m";

    static int Glacier30Command(int seeds, int first, bool dbg)
    {
        // the GREEN gate is the glide case (design.n30.glacier30 + design.n30.glide, both on in the build); the no-glide chaser is printed as information
        GameConfig cfgOff = Cfg30(), cfgOn = Cfg30("glacier30", "glide"), cfgNoGlide = Cfg30("glacier30");
        double dt = cfgOn.Dt;
        var fails = new List<string>();
        double hideFill = cfgOn.Num("design.n30.glacier30.hideSeconds") + cfgOn.Num("design.n30.glacier30.fillSeconds");
        // RED: the row off: NIGHT29's rink, the bank camper's quiet time (GG: "the slope section where it literally can't attack you")
        GlacierRow30 r0 = GlacierRun30(cfgOff, (uint)first, 120 * 120, 1, false, dbg);
        Console.WriteLine($"  RED row OFF bank camper (immortal, 120 s; {r0.bankAt}): {GlacierLine30(r0, dt)}");
        GlacierRow30 c0 = GlacierRun30(cfgOff, (uint)first, 240 * 120, 0, false, dbg);
        Console.WriteLine($"  RED row OFF chaser seed {first}: {GlacierLine30(c0, dt)}");
        // GREEN: the bank camper on the rebuilt rink
        GlacierRow30 r1 = GlacierRun30(cfgOn, (uint)first, 120 * 120, 1, true, dbg, true);
        Console.WriteLine($"  GREEN bank camper (glide on, immortal, 120 s; {r1.bankAt}): {GlacierLine30(r1, dt)}");
        if (r1.killed) fails.Add("the bank camper killed the glacier");
        if (r1.longestOut > hideFill + 0.5) fails.Add($"the bank camper was out of reach {r1.longestOut:0.0} s (limit hideSeconds + fillSeconds = {hideFill:0} s)");
        if (r1.longestQuiet >= r0.longestQuiet) fails.Add($"the bank is no less safe: quiet {r1.longestQuiet:0.0} s vs {r0.longestQuiet:0.0} s with the row off");
        if (r1.volleys == 0) fails.Add("the bank camper was never volleyed");
        // the fleer: runs away round the lap for 60 s (glide on): distance must be punished by volleys
        GlacierRow30 fl = GlacierRun30(cfgOn, (uint)first, 60 * 120, 2, true, dbg, true);
        Console.WriteLine($"  GREEN fleer (glide on, immortal, 60 s, runs away round the lap): {GlacierLine30(fl, dt)}");
        if (fl.volleys == 0) fails.Add("the fleer was never volleyed");
        // GREEN: the chaser lag bot's kill band
        var times = new List<double>();
        double outWorst = 0;
        for (uint sd = (uint)first; sd < (uint)(first + seeds); sd++)
        {
            GlacierRow30 r = GlacierRun30(cfgOn, sd, 240 * 120, 0, true, dbg, true);
            if (r.steps < 0) { fails.Add($"seed {sd}: Glacier28Begin failed"); continue; }
            Console.WriteLine($"  GREEN GLIDE chaser seed {sd}: {GlacierLine30(r, dt)}; glide steps {r.glideSteps}, dash adds {r.dashAdds}");
            if (r.killed) times.Add(r.steps * dt); else fails.Add($"chaser seed {sd} not killed in 240 s");
            outWorst = Math.Max(outWorst, r.longestOut);
        }
        double med = Median30(times);
        if (times.Count > 0 && (med < 80 || med > 130)) fails.Add($"the chasers' median {med:0} s is outside 80-130 s");
        // information: the same chaser with glide off (the NIGHT28 ice; the window numbers then are NIGHT29's)
        {
            var gt = new List<double>();
            for (uint sd = (uint)first; sd < (uint)(first + Math.Min(seeds, 3)); sd++)
            {
                GlacierRow30 r = GlacierRun30(cfgNoGlide, sd, 240 * 120, 0, true, dbg, false);
                Console.WriteLine($"  info NO-GLIDE chaser seed {sd}: {GlacierLine30(r, dt)}");
                if (r.killed) gt.Add(r.steps * dt);
            }
            Console.WriteLine($"  info NO-GLIDE summary: {gt.Count} kills, median {Median30(gt):0} s (not a gate)");
        }
        if (r1.rinkW * r1.rinkL < 2 * 28 * 52) fails.Add($"the rink is {r1.rinkW:0} x {r1.rinkL:0} m, under twice 28 x 52");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss30 glacier {(fails.Count == 0 ? "PASS" : "FAIL")}: GLIDE chaser median {med:0} s over {times.Count} kills (band 80-130 s); bank camper longest out of reach {r1.longestOut:0.0} s (limit {hideFill:0}), quiet {r1.longestQuiet:0.0} s (row off {r0.longestQuiet:0.0} s); chasers' worst out of reach {outWorst:0.0} s; rink {r1.rinkW:0} x {r1.rinkL:0} m");
        return fails.Count == 0 ? 0 : 1;
    }
}
