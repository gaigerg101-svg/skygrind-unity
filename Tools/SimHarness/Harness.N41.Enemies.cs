// NIGHT41 builder E6 (NIGHT41.md card E6): the enemies' NIGHT41 rows. One entry, N41Enemies (declared and called in Program.cs). Commands:
//   n41foes level --level path [--seeds n] [--first s] [--cap s] [--tokens off|on]
//     tokens41's measure (INVENTORY A5; E3's `n40foes level` with the run's NIGHT41 blocks set): the walk bot (Walk29Pilot + Foes40WalkInput)
//     through a whole campaign level, the design's NIGHT40 rows (NIGHT40's game), tokens41 on (or off: the before), the player kept alive.
//     Per room: cleared k / n and the median s; over the level: the most enemies in an attack at once (Husk.committed) and the share of
//     fight time with more than two, the brain's cap waits. PASS: every room cleared on every seed and (tokens on) never more than the cap.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N41Enemies(string cmd, string[] args, ref int code)
    {
        if (cmd != "n41foes") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "level";
        if (what == "level") { code = Foes41Level(args); return; }
        if (what == "heavy") { code = Foes41Heavy(args); return; }
        if (what == "walk")
        {
            // `n41foes walk [n40ride walk args] [--tokens off]`: R2's walk (Walk38Pilot, every playable room of each level, the design config) with
            // tokens41 forced on (off: forced off), the most enemies in an attack at once over the run printed by the brain's cap waits.
            // --kiln41: kiln41 on too (pass the level id n41-kiln: N41BaseId maps it to n40-kiln for the walk pilots and NIGHT40's kiln rules).
            bool off = Opt40F(args, "--tokens", "on") == "off", kiln = Array.IndexOf(args, "--kiln41") >= 0;
            var a = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--tokens") { i++; continue; }
                if (args[i] == "--kiln41") continue;
                a.Add(i == 0 ? "n40ride" : args[i]);
            }
            SurvivalSim.Force41 = new N41Blocks { tokens41 = !off, kiln41 = kiln };
            try { code = N40WalkCommand(a.ToArray()); }
            finally { SurvivalSim.Force41 = null; }
            Console.WriteLine($"n41foes walk: tokens41 forced {(off ? "OFF" : "ON")}, kiln41 {(kiln ? "ON" : "off")}: {(code == 0 ? "GREEN" : "RED")}");
            return;
        }
        if (what == "kiln")
        {
            // `n41foes kiln [--off] [--seeds N] [--first N] [--trace]`: the kiln walk (walk29 kiln: Walk29Pilot with Bot/Walk38Kiln's scripts, as
            // `n38kiln walk`) on n41-kiln with kiln41 and tokens41 forced on; --off: on n40-kiln with both off (NIGHT40's level, the control).
            bool off = Array.IndexOf(args, "--off") >= 0;
            var rest = new List<string> { "walk29", "kiln" };
            for (int i = 2; i < args.Length; i++) if (args[i] != "--off") rest.Add(args[i]);
            if (!rest.Contains("--seeds")) { rest.Add("--seeds"); rest.Add("10"); }
            string old = walkPrefix;
            walkPrefix = off ? "n40" : "n41";
            SurvivalSim.Force41 = off ? N41Blocks.Off : new N41Blocks { tokens41 = true, kiln41 = true };
            try { N29Walk("walk29", rest.ToArray(), ref code); }
            finally { walkPrefix = old; SurvivalSim.Force41 = null; }
            Console.WriteLine($"n41foes kiln: {(off ? "n40-kiln, tokens41 and kiln41 OFF" : "n41-kiln, tokens41 and kiln41 ON")}: {(code == 0 ? "GREEN" : "RED")}");
            return;
        }
        if (what == "s1kiln")
        {
            // `n41foes s1kiln [--tokens41] [--kiln41] [n40start kiln args]`: NIGHT40 S1's kiln walk (`n40start kiln --seeds 10 --start40`: Walk29Pilot +
            // Walk38Kiln, kiln40 forced on, its 10/10 in 438e549d) with the run's NIGHT41 blocks forced: none (the control), tokens41, or kiln41 +
            // tokens41 (--kiln41 walks n41-kiln: N41BaseId maps it to n40-kiln for the pilot and NIGHT40's kiln rules).
            bool tok = Array.IndexOf(args, "--tokens41") >= 0, kiln = Array.IndexOf(args, "--kiln41") >= 0;
            var rest = new List<string> { "n40start", "kiln" };
            for (int i = 2; i < args.Length; i++) if (args[i] != "--tokens41" && args[i] != "--kiln41") rest.Add(args[i]);
            if (kiln && !rest.Contains("--level")) { rest.Add("--level"); rest.Add("n41-kiln"); }
            SurvivalSim.Force41 = new N41Blocks { tokens41 = tok, kiln41 = kiln };
            try { code = Kiln40Walk(rest.ToArray()); }
            finally { SurvivalSim.Force41 = null; }
            Console.WriteLine($"n41foes s1kiln: tokens41 {(tok ? "ON" : "off")}, kiln41 {(kiln ? "ON (n41-kiln)" : "off (n40-kiln)")}: {(code == 0 ? "GREEN" : "RED")}");
            return;
        }
        Console.WriteLine($"n41foes: unknown sub-command '{what}' (level | heavy | walk | kiln | s1kiln)");
        code = 2;
    }

    /// <summary>`n41foes heavy [--kinds husk,tank,warden,charger] [--level path] [--seeds n]`: heavy41's line per kind. One enemy of the kind in the
    /// level's first wave room (n37-iceline, E3's TTK room; a TANK is a husk with tank29 set at its spawn, as FeelReel.N40.Foes does), the player
    /// invulnerable, walking into 2 m and swinging the gauntlet until its FIRST hit on the body, then standing still. Measured with heavy41 off and
    /// on (loadout.heavy41 set after the run's start): the body's horizontal shove 0.6 s after the hit (m), the knock's hold (s: a knocked CHARGER / WARDEN stands still, its step holds it like a stun, so the hold is its answer), the hitstop the swing asked (s, the
    /// sim's Hitstop through loadout.hitstop41), the heavy41 event. PASS: every big kind is shoved farther or held longer, and freezes longer, with it on (the TANK dies to one swing, and a swing into the WARDEN's shield is its NIGHT40 shield break, a stun with no knock: the hitstop alone); the husk
    /// (not big) is unchanged.</summary>
    static int Foes41Heavy(string[] args)
    {
        string[] kinds = Opt40F(args, "--kinds", "husk,tank,warden,charger").Split(',');
        string path = Opt40F(args, "--level", "Design/levels/n37-iceline.json");
        int seeds = int.Parse(Opt40F(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        Console.WriteLine($"n41foes heavy: the gauntlet's first hit on one enemy per kind ({path}'s first wave room), {seeds} seeds; shove = the body's horizontal move 0.6 s after the hit (median), hitstop = the sim's freeze the swing asked");
        int bad = 0;
        foreach (string kind in kinds)
        {
            var shove = new double[2];
            var stop = new double[2];
            var evs = new int[2];
            var held = new double[2];
            for (int on = 0; on < 2; on++)
            {
                var sh = new List<double>();
                for (int s = 0; s < seeds; s++)
                {
                    var (m, hs, ev, hd) = Heavy41Run(path, kind, on == 1, (uint)(4100 + s));
                    held[on] = Math.Max(held[on], hd);
                    sh.Add(m);
                    stop[on] = Math.Max(stop[on], hs);
                    evs[on] += ev;
                }
                sh.Sort();
                shove[on] = sh[sh.Count / 2];
            }
            bool big = kind != "husk" && kind != "swarmer" && kind != "spitter";
            bool ok = big ? (shove[1] > shove[0] || held[1] > held[0] || kind == "tank" || kind == "warden") &&stop[1] > stop[0] && evs[1] > 0 : shove[1] == shove[0] && stop[1] == stop[0] && evs[1] == 0;
            if (!ok) bad++;
            Console.WriteLine($"  {kind,-8} off: shove {shove[0]:0.00} m, held {held[0]:0.00} s, hitstop {stop[0]:0.000} s | on: shove {shove[1]:0.00} m, held {held[1]:0.00} s, hitstop {stop[1]:0.000} s, heavy41 events {evs[1]} ({(big ? "big" : "not big")}): {(ok ? "PASS" : "RED")}");
        }
        Console.WriteLine($"n41foes heavy {(bad == 0 ? "PASS" : "RED")}");
        return bad == 0 ? 0 : 1;
    }

    static (double shove, double hitstop, int events, double held) Heavy41Run(string path, string kind, bool on, uint seed)
    {
        Core36Keep.Clear();
        string spawnKind = kind == "tank" ? "husk" : kind;
        SurvivalSim sim = Foes40Sim(path, null, new List<object> { new List<object> { spawnKind } }, new N40Blocks { foes40 = true }, seed, out RoomDef room, noPickup: true);
        sim.mover.invulnerable = true;
        sim.loadout.heavy41 = on;
        double asked = 0;
        int events = 0;
        Action<double, string> orig = sim.loadout.hitstop41;
        sim.loadout.hitstop41 = (d, c) => { asked = Math.Max(asked, d); orig?.Invoke(d, c); };
        sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
        double yaw = room.spawnYawDeg * Math.PI / 180, pitch = 0;
        List<Husk> list = sim.husks.PoolOf(spawnKind);
        for (int w = 0; w < 120 && !(list.Count > 0 && list[0].alive); w++) Core36TickAll(sim, yaw);
        Husk h = list.Count > 0 && list[0].alive ? list[0] : null;
        if (h == null) return (-2, 0, 0, 0);
        if (kind == "tank") h.tank29 = true;
        sim.loadout.Reset(new List<string> { "gauntlet" }, seed);
        Core36Keep.Add(h);
        sim.events.On("heavy41", o => { if ((int)o == h.id) events++; });
        bool hit = false;
        Vec3 at = default;
        int hitStep = -1;
        double heldS = 0;
        sim.events.On<HitEvent>("hit", e => { if (!hit && e.enemy == h.id && e.weapon == "gauntlet") { hit = true; at = new Vec3(h.position.x, h.position.y, h.position.z); } });
        int max = MovementMath.StepsFor(20, sim.step), after = MovementMath.StepsFor(0.6, sim.step);
        for (int i = 0; i < max; i++)
        {
            if (hit && hitStep < 0) { hitStep = i; heldS = h.alive ? h.knockSteps * sim.step : 0; }
            if (hitStep >= 0 && i - hitStep >= after) break;
            Vec3 eye = sim.Eye, me = sim.mover.position;
            Vec3 v = h.AimPoint - eye;
            if (v.Length > 1e-6) { yaw = JsMath.Atan2(-v.x, -v.z); pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z)); }
            var f = new InputFrame();
            if (!hit)
            {
                Vec3 to = h.position - me;
                double gl = JsMath.Hypot(to.x, to.z);
                if (gl > 2.0)
                {
                    Vec3 want = new Vec3(to.x / gl, 0, to.z / gl);
                    Vec3 fwd = MovementMath.WishDirection(0, 1, yaw), right = MovementMath.WishDirection(1, 0, yaw);
                    f.moveZ = want.x * fwd.x + want.z * fwd.z;
                    f.moveX = want.x * right.x + want.z * right.z;
                }
            }
            bool press = !hit && i % 2 == 0;
            Core36Tick(sim, yaw, pitch, new CombatInput { weapon = -1, fire = new Button { pressed = press, held = press } }, f);
        }
        if (!hit) return (-1, asked, events, 0);
        return (JsMath.Hypot(h.position.x - at.x, h.position.z - at.z), asked, events, heldS);
    }

    static int Foes41Level(string[] args)
    {
        string path = Opt40F(args, "--level", null);
        if (path == null) { Console.WriteLine("n41foes level: --level <path>"); return 2; }
        int seeds = int.Parse(Opt40F(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        int first = int.Parse(Opt40F(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(Opt40F(args, "--cap", "600"), CultureInfo.InvariantCulture);
        bool tokens = Opt40F(args, "--tokens", "on") != "off";
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var times = new List<double>[playable];
        var cleared = new int[playable];
        for (int i = 0; i < playable; i++) times[i] = new List<double>();
        int maxCommitted = 0, waits = 0, windupCap = 0;
        long fightSteps = 0, overTwo = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalSim sim = SurvivalSim.Headless(Config(), SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n41Override = new N41Blocks { tokens41 = tokens };
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new Walk29Pilot(sim, ap);
            sim.UseInput(new Foes40WalkInput(pilot));
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { cleared[e.room]++; times[e.room].Add(e.time); } });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(cap, sim.step);
            for (int st = 0; st < max; st++)
            {
                sim.health.hp = sim.health.max;
                if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; }
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                windupCap = Math.Max(windupCap, sim.brain.windupCap41);
                if (sim.campaign != null && sim.campaign.phase == Campaign24.Phase.Fight)
                {
                    int n = 0;
                    foreach (Enemy e in sim.enemies.all) if (e.alive && e is Husk h && h.committed) n++;
                    fightSteps++;
                    if (n > 2) overTwo++;
                    if (n > maxCommitted) maxCommitted = n;
                }
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            waits += sim.brain.capWaits41;
            ap.Detach();
        }
        int bad = 0;
        for (int i = 0; i < playable; i++)
        {
            times[i].Sort();
            if (cleared[i] < seeds) bad++;
            Console.WriteLine($"  room {level.rooms[i].id,-4} {level.rooms[i].name,-18} cleared {cleared[i]}/{seeds}, median {(times[i].Count > 0 ? times[i][times[i].Count / 2].ToString("0.0", CultureInfo.InvariantCulture) + " s" : "-")}");
        }
        bool capOk = !tokens || maxCommitted <= windupCap;
        Console.WriteLine($"n41foes level {System.IO.Path.GetFileNameWithoutExtension(path)} tokens41 {(tokens ? "on" : "off")} (cap {windupCap}): {playable - bad}/{playable} rooms cleared on every seed ({seeds}); the most in an attack at once {maxCommitted}, more than two for {(fightSteps > 0 ? 100.0 * overTwo / fightSteps : 0).ToString("0.00", CultureInfo.InvariantCulture)} % of the fight time ({fightSteps} steps); cap waits {waits}: {(bad == 0 && capOk ? "PASS" : "RED")}");
        return bad == 0 && capOk ? 0 : 1;
    }
}
