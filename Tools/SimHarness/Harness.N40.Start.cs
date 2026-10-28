// NIGHT40 builder S1 (start40, kiln40, cut40, rods40): the S1 harness commands, declared and called by the lead's Program.cs (N40Start).
//   n40start grace [--on] [--seconds S] [--seed N]
//       The root-cause line (NIGHT40.md method 3): per campaign level 1-5 (Curriculum25's order as the config gives it), with two pilots (idle:
//       no input; walk: walk29's pilot, the walk bot), the seconds from the run's first live step to the first enemy ABLE TO ATTACK (alive,
//       not a stand-in, not staggered / stunned / knocked, and a husk-family body with its attack cooldown at 0), the first damage the player
//       takes, and how the start40 hold ended. --on forces design.n40.start40 on for the run (N40Blocks override, the row's own numbers);
//       without it the run takes the design's bits (--off40: every NIGHT40 block off).
//   n40start retry [--on] [--seed N]
//       After a respawn: the walk pilot plays each level until its first wave is in, the player is killed, and the line prints the seconds
//       from the retry (roomRetry) to the first enemy able to attack again.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N40Start(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40start") return;
        string sub = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "grace";
        if (sub == "grace") { code = Start40Grace(args, false); return; }
        if (sub == "retry") { code = Start40Grace(args, true); return; }
        if (sub == "kiln") { code = Kiln40Walk(args); return; }
        if (sub == "rods") { code = Rods40Scan(args); return; }
        if (sub == "cut") { code = Cut40Check(); return; }
        Console.WriteLine("n40start: unknown sub-command " + sub + " (grace | retry | kiln | rods | cut)");
        code = 2;
    }

    static string Arg40(string[] args, string name, string def)
    {
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return def;
    }

    /// <summary>Can this enemy start an attack now (the measure's definition)?</summary>
    static bool Start40Able(Enemy e)
    {
        if (!e.alive || e.standIn29 || e.staggerSteps > 0 || e.knockSteps > 0) return false;
        if (e is Husk h && h.cooldownSteps > 0) return false;
        return true;
    }

    /// <summary>`n40start kiln [--seeds N] [--first N] [--cap S] [--start40] [--trace]`: walk29's pilot (Walk29Pilot + Walk38Kiln, which answers for
    /// n40-kiln) on Design/levels/n40-kiln.json with design.n40.kiln40 forced on (+ start40 with --start40): every playable room cleared per seed (the
    /// walk 10/10), the weapons owned at the start and on entering each room (seed `first`), and the boost chain's lines as the sim emitted them.</summary>
    static int Kiln40Walk(string[] args)
    {
        int seeds = int.Parse(Arg40(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        int first = int.Parse(Arg40(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(Arg40(args, "--cap", "900"), CultureInfo.InvariantCulture);
        bool start40 = Array.IndexOf(args, "--start40") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        GameConfig cfg = Config();
        string id = Arg40(args, "--level", "n40-kiln");
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var cleared = new int[playable];
        var deaths = new int[playable];
        var times = new List<double>();
        int done = 0;
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        Console.WriteLine($"n40start kiln: {id}, kiln40 FORCED ON{(start40 ? " + start40" : "")}, {seeds} seeds from {first}, cap {cap} s; awards {JsonOf40(cfg, "design.n40.kiln40.awards")}");
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n40Override = new N40Blocks { kiln40 = true, start40 = start40 };
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            sim.UseInput(new Walk29Pilot(sim, ap));
            bool print = s == 0;
            int st = 0;
            sim.events.On<RoomEvent>("roomEnter", e => { if (print) Console.WriteLine($"  [seed {seed}] {st * sim.step,7:0.00} s enter {e.id}: owned {string.Join(", ", sim.loadout.owned)}"); });
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) cleared[e.room]++; if (trace) Console.WriteLine($"  [seed {seed}] {st * sim.step,7:0.00} s clear {e.id}"); });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) deaths[e.room]++; });
            sim.events.On<WeaponEarnedEvent>("weaponEarned", w => { if (print) Console.WriteLine($"  [seed {seed}] {st * sim.step,7:0.00} s weaponEarned {w.weapon} (room {w.room})"); });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(cap, sim.step);
            bool shownStart = false;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (print && !shownStart && sim.campaign != null && st >= 2) { shownStart = true; Console.WriteLine($"  [seed {seed}] {st * sim.step,7:0.00} s the level's start: owned {string.Join(", ", sim.loadout.owned)}, in hand {sim.loadout.Weapon} (n40 {sim.n40})"); }
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (ok) { done++; times.Add(st * sim.step); }
            Console.WriteLine($"  seed {seed}: {(ok ? "CLEARED" : "STOPPED")} at {st * sim.step:0.0} s, room {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")}; chain {string.Join(" | ", sim.kiln40Shown)}");
            ap.Detach();
        }
        for (int i = 0; i < playable; i++) Console.WriteLine($"  {level.rooms[i].id}: cleared {cleared[i]}/{seeds}, deaths {deaths[i]}");
        times.Sort();
        Console.WriteLine($"n40start kiln: {(done == seeds ? "GREEN" : "RED")} {done}/{seeds} runs cleared every playable room (median {(times.Count > 0 ? times[times.Count / 2] : double.NaN):0.0} s)");
        return done == seeds ? 0 : 1;
    }

    /// <summary>`n40start rods [--level id] [--list]`: rods40's scan (Rods40.cs, the views' own test): per campaign level 1-5 (and n40-kiln), the drawn
    /// pieces with no collider inside the play volume today: dress33's decor props (by kind), the light plan's fixtures (shafts, pole lamps, ...;
    /// LightPlan33.Green with look30 and lights35 as the config has them) and, on a spire level, crown38's spire slits. "after" counts what the views still
    /// draw in the play volume with rods40 on (the same test skips each one: 0 by construction; the line proves the filter covers every source).</summary>
    static int Rods40Scan(string[] args)
    {
        GameConfig cfg = Config();
        bool list = Array.IndexOf(args, "--list") >= 0;
        string only = Arg40(args, "--level", null);
        LightPlan33.Lights35On = c => c.Design("n35.lights35.enabled");
        bool look30 = cfg.Design("n30.look30.enabled"), crown38 = cfg.Design("n38.crown38.enabled");
        var ids = new List<string>(Curriculum25.Order(cfg, N25Blocks.From(cfg)));
        if (ids.Count > 5) ids.RemoveRange(5, ids.Count - 5);
        ids.Add("n40-kiln");
        Console.WriteLine($"n40start rods: play volume {UnityTuning.Rods40FloorMin}-{UnityTuning.Rods40FloorMax} m over a floor, > {UnityTuning.Rods40WallClear} m from a wall or a ceiling, inside a room; look30 {look30}, lights35 {cfg.Design("n35.lights35.enabled")}, crown38 {crown38}");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        int total = 0;
        foreach (string id in ids)
        {
            if (only != null && only != id) continue;
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            if (!File.Exists(path)) continue;
            var json = PortData.Obj(PortData.LoadJson(path));
            LightPlan33.Level lv = LightPlan33.Read(json, id);
            LightPlan33.Nums nums = LightPlan33.Nums.From(cfg);
            nums.look30 = look30;
            nums.lights35 = LightPlan33.Lights35On(cfg);
            LightPlan33.Plan plan = LightPlan33.Green(lv, nums);
            var byKind = new SortedDictionary<string, int>();
            var drawn = new SortedDictionary<string, int>();
            int hidden = 0, pieces = 0;
            void Count(string kind, bool hide, string where)
            {
                pieces++;
                drawn.TryGetValue(kind, out int d); drawn[kind] = d + 1;
                if (!hide) return;
                hidden++;
                byKind.TryGetValue(kind, out int n); byKind[kind] = n + 1;
                if (list) Console.WriteLine($"    {id} {kind} {where}");
            }
            var campaign = PortData.Obj(json["campaign"]);
            if (campaign.TryGetValue("props", out object po) && po is List<object> rows)
                foreach (object ro in rows)
                {
                    if (!(ro is Dictionary<string, object> r) || !Rods40.NoCollider(r) || Rods40.IsPickup(r) || !Rods40.PropBox(r, out LightPlan33.Box b)) continue;
                    bool hide = Rods40.HideProp(lv, r, out Vec3 at);
                    Count("prop " + (r.TryGetValue("kind", out object k) ? k : "?"), hide, $"{r["id"]} room {r["room"]} box ({b.x0:0.0},{b.y0:0.0},{b.z0:0.0})-({b.x1:0.0},{b.y1:0.0},{b.z1:0.0}) in play at ({at.x:0.0}, {at.y:0.0}, {at.z:0.0})");
                }
            var poleDone = new HashSet<string>();
            foreach (LightPlan33.Fix f in plan.fixes)
            {
                string kind = "fix " + (f.kind == "beam" ? "beam (shaft)" : f.type == "pole" ? "pole lamp" : f.kind + "/" + f.type);
                if (f.type == "pole" && !poleDone.Add(f.id)) continue; // one count per pole lamp (its plate, shaft and head)
                bool hide = Rods40.HideFix(lv, f, plan);
                string where = f.kind == "beam" ? $"{f.id} room {f.room} ({f.tx:0.0},{f.ty:0.0},{f.tz:0.0}) -> ({f.bx:0.0},{f.by:0.0},{f.bz:0.0})" : $"{f.id} room {f.room} box ({f.box.x0:0.0},{f.box.y0:0.0},{f.box.z0:0.0})-({f.box.x1:0.0},{f.box.y1:0.0},{f.box.z1:0.0})";
                Count(kind, hide, where);
            }
            // crown38's spire slits (Crown37View.Crown38.cs: 0.12 m white boxes up the core's four corners, every slitEvery m, 0.03 m off the core)
            if (crown38)
            {
                SurvivalMap map = SurvivalMap.FromExport(json);
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noRecords = sim.noLab = true;
                sim.setup.mode = Campaign24.Mode;
                sim.StartRun(1, null, null);
                for (int s = 0; s < 3; s++) sim.Step();
                if (sim.spire37Row != null && sim.spire37Row.TryGetValue("coreHalf", out object ch))
                {
                    double half = PortData.Num(ch), top = sim.spire37Axis.y, bottom = sim.spire37Checkpoint[2].y;
                    double every = cfg.Num("design.n38.crown38.slitEvery");
                    for (double y = top - every / 2; y > bottom + 1; y -= every)
                        foreach (var (sx, sz) in new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) })
                        {
                            double x = sim.spire37Axis.x + sx * (half + 0.03), z = sim.spire37Axis.z + sz * (half + 0.03);
                            bool hide = Rods40.Hide(lv, x, y, z, 0.06, every * 0.45 / 2, 0.06);
                            Count("crown38 slit", hide, $"({x:0.0}, {y:0.0}, {z:0.0})");
                        }
                }
            }
            total += hidden;
            // after: the game's own path with the row on (LightPlan33.GreenFor -> Rods40Filter); the props' path is PropsView29's HideProp (the test above)
            GameConfig on = Config();
            PortData.Obj(PortData.Obj(PortData.Obj(on.root["design"])["n40"])["rods40"])["enabled"] = true;
            LightPlan33.Plan after = LightPlan33.GreenFor(json, on, look30, out LightPlan33.Level lvAfter);
            int left = 0;
            foreach (LightPlan33.Fix f in after.fixes) if (Rods40.HideFix(lvAfter, f, after)) left++;
            int poleLights = 0;
            foreach (LightPlan33.Lit l in after.lights) if (l.type == "pole") poleLights++;
            Console.WriteLine($"  {id}: {hidden} of {pieces} collider-less pieces in the play volume (before); after (rods40 on, the game's plan): {left} fixtures left in it, {plan.fixes.Count - after.fixes.Count} fixtures taken out, pole lights {poleLights}");
            foreach (var kv in byKind) Console.WriteLine($"      {kv.Key}: {kv.Value} of {drawn[kv.Key]}");
        }
        Console.WriteLine($"n40start rods: {total} pieces in the play volume today, every one skipped by the rods40 filter");
        return 0;
    }

    /// <summary>`n40start cut`: cut40 off (the config) and forced on (in memory): the campaign order (Curriculum25), the level select's slots as
    /// LevelSel26.Params builds them (UiLevelSelect26.cs: campaign6's 6 playable, then cut40's UnityTuning.Cut40 numbers; repeated here: the Ui file
    /// is not in the harness) through Core's LevelSelect26.Slots with design.maps.list's labels, and level 5's results head (Curriculum25.Next null:
    /// "CAMPAIGN COMPLETE", UiCampaignResults25). PASS with the row on: six slots, five playable, the sixth LOCKED and unnamed, no "LEVEL 6..11".</summary>
    static int Cut40Check()
    {
        bool ok = true;
        foreach (bool on in new[] { false, true })
        {
            GameConfig cfg = Config();
            if (on) PortData.Obj(PortData.Obj(PortData.Obj(cfg.root["design"])["n40"])["cut40"])["enabled"] = true;
            N25Blocks n25 = N25Blocks.From(cfg);
            List<string> order = Curriculum25.Order(cfg, n25);
            var labels = new Dictionary<string, string>();
            foreach (object o in cfg.Arr("design.maps.list")) { var d = PortData.Obj(o); labels[(string)d["id"]] = (string)d["label"]; }
            LevelSelect26Params p = LevelSelect26Params.From(cfg);
            if (cfg.Design("n35.campaign6.enabled") && p.playable < 6) p.playable = 6; // UiLevelSelect26: LevelSel35Playable
            bool cut = !N40Blocks.ForceOff && cfg.Design("n40.cut40.enabled");
            if (cut) { p.slots = UnityTuning.LevelSel40Slots; if (p.playable > UnityTuning.LevelSel40Playable) p.playable = UnityTuning.LevelSel40Playable; }
            List<LevelSlot26> slots = LevelSelect26.Slots(order, null, p, id => labels.ContainsKey(id), id => labels.TryGetValue(id, out string l) ? l : null, id => null);
            Console.WriteLine($"cut40 {(on ? "ON (forced)" : "off (config)")}: order {string.Join(", ", order)}");
            int playable = 0;
            bool shows6 = false;
            foreach (LevelSlot26 s in slots)
            {
                if (s.playable) playable++;
                string shown = s.number + "  " + (s.exists ? s.name : "LOCKED");
                if (s.exists && s.number >= 6) shows6 = true;
                Console.WriteLine($"    slot {shown}{(s.playable ? "" : " (locked)")}");
            }
            string last = order.Count >= 5 ? order[4] : "?";
            string head = Curriculum25.Next(cfg, n25, last) == null ? "CAMPAIGN COMPLETE" : "LEVEL 5 CLEARED (next " + Curriculum25.Next(cfg, n25, last) + ")";
            Console.WriteLine($"  slots {slots.Count}, playable {playable}; level 5 ({last}) results head: {head}");
            if (on) ok = slots.Count == 6 && playable == 5 && !slots[5].exists && !shows6 && head == "CAMPAIGN COMPLETE" && order.Count == 5;
        }
        Console.WriteLine($"n40start cut: {(ok ? "PASS" : "FAIL")}");
        return ok ? 0 : 1;
    }

    static string JsonOf40(GameConfig cfg, string key)
    {
        if (!cfg.Has(key)) return "none";
        var sb = new System.Text.StringBuilder();
        foreach (object o in cfg.Arr(key))
        {
            var d = PortData.Obj(o);
            sb.Append(sb.Length > 0 ? ", " : "").Append(d["room"]).Append(" -> ").Append(d["weapon"]);
        }
        return sb.ToString();
    }

    static int Start40Grace(string[] args, bool retry)
    {
        bool on = Array.IndexOf(args, "--on") >= 0;
        double secs = double.Parse(Arg40(args, "--seconds", retry ? "120" : "20"), CultureInfo.InvariantCulture);
        uint seed = uint.Parse(Arg40(args, "--seed", "1"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        List<string> order = Curriculum25.Order(cfg, N25Blocks.From(cfg));
        Console.WriteLine($"n40start {(retry ? "retry" : "grace")}: start40 {(on ? "FORCED ON" : N40Blocks.ForceOff ? "off (--off40)" : "design (" + (cfg.Design("n40.start40.enabled") ? "on" : "off") + ")")}, seed {seed}, order {string.Join(", ", order)}");
        if (on) Console.WriteLine($"  start40 numbers: moveMetres {cfg.Num("design.n40.start40.moveMetres")}, holdSeconds {cfg.Num("design.n40.start40.holdSeconds")}, noAttackSeconds {cfg.Num("design.n40.start40.noAttackSeconds")}; spawns33 tellSeconds {cfg.Num("design.n33.spawns33.tellSeconds")}");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        string[] pilots = retry ? new[] { "walk" } : new[] { "idle", "walk" };
        for (int li = 0; li < Math.Min(5, order.Count); li++)
        {
            string id = order[li];
            string only = Arg40(args, "--level", null);
            if (only != null && only != (li + 1).ToString(CultureInfo.InvariantCulture) && only != id) continue;
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            if (!File.Exists(path)) { Console.WriteLine($"  L{li + 1} {id}: no file {path}"); continue; }
            foreach (string pilot in pilots)
            {
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
                AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
                AutopilotInput.GraphSource = BotNav.Survival;
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noRecords = sim.noLab = true;
                sim.setup.mode = Campaign24.Mode;
                if (on) sim.n40Override = new N40Blocks { start40 = true };
                AutopilotInput ap = null;
                if (pilot == "walk")
                {
                    ap = new AutopilotInput(sim, "normal", seed);
                    ap.GoalProvider = () =>
                    {
                        Campaign24 c = sim.campaign;
                        LevelDef lv = sim.campaignLevel;
                        if (c == null || c.room < 0) return lv.rooms[0].spawn;
                        if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                        return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
                    };
                    if (id == "n38-foundry" || id == "n38-finale") sim.UseInput(new Walk38Pilot(sim, ap)); // n38f walk's wrapper (Harness.N38.Foundry.cs)
                    else sim.UseInput(new Walk29Pilot(sim, ap));
                }
                // the level's first fighting room (waves, not a boss): when it is not room 0 (a link first: level 1's a1, level 5's v1), the player is
                // put at its spawn once the campaign runs (doors free, as n38f walk does past the rooms it does not play): the walk bot does not cross a1
                LevelDef lvDef = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
                int k = lvDef.rooms.FindIndex(r => r.WaveCount > 0 && r.kind != "boss");
                string release = "-";
                int retryAt = -1, cur = 0;
                string retryRoom = "";
                int entered = -1, spawned = -1;
                string enteredId = k >= 0 ? lvDef.rooms[k].id : "?";
                sim.events.On<RoomEvent>("roomEnter", e => { if (entered < 0 && e.room == k) entered = cur; if (Array.IndexOf(args, "--debug") >= 0) Console.WriteLine($"    enter {e.id} at step {cur}"); });
                bool dbg = Array.IndexOf(args, "--debug") >= 0;
                sim.events.On<SpawnEvent>("enemySpawn", e => { if (spawned < 0 && entered >= 0) spawned = cur; if (dbg) Console.WriteLine($"    spawn {e.kind} id {e.id} at step {cur} wave {sim.campaign?.Wave33}"); });
                sim.events.On<string>("start40Release", s => { release = s; if (dbg) Console.WriteLine($"    start40Release {s} at step {cur}"); });
                sim.events.On<RoomEvent>("roomRetry", e => { if (retryAt < 0) { retryAt = cur; retryRoom = e.id; } });
                sim.StartRun(seed, null, null);
                int max = MovementMath.StepsFor(secs, sim.step);
                int live = -1, able = -1, hurt = -1, killed = -1, ableAfter = -1;
                string firstRoom = "", ableKind = "";
                bool placed = k <= 0;
                for (int st = 0; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
                {
                    if (ap != null) AutopilotInput.TakeDraft(sim);
                    double hpBefore = sim.health.hp;
                    cur = st;
                    sim.Step();
                    Campaign24 c = sim.campaign;
                    if (c == null) continue;
                    if (!placed && c.room >= 0)
                    {
                        c.SetDoorFree29(true);
                        sim.Respawn(lvDef.rooms[k].spawn, lvDef.rooms[k].spawnYawDeg);
                        placed = true;
                    }
                    if (entered < 0) continue;
                    if (live < 0) live = entered;
                    if (Array.IndexOf(args, "--debug") >= 0 && (st - live) % 240 == 120)
                        foreach (Enemy e in sim.enemies.all)
                            if (e.alive) Console.WriteLine($"    t {(st - live) * sim.step:0.0} {e.kind} id {e.id} stagger {e.staggerSteps} knock {e.knockSteps} cd {(e is Husk hh ? hh.cooldownSteps : -1)} standIn {e.standIn29} at ({e.position.x:0.0}, {e.position.y:0.0}, {e.position.z:0.0})");
                    bool any = false;
                    string kind = "";
                    foreach (Enemy e in sim.enemies.all) if (Start40Able(e)) { any = true; kind = e.kind + (e.tank29 ? "(tank)" : ""); break; }
                    if (!retry)
                    {
                        if (able < 0 && any) { able = st; ableKind = kind; firstRoom = c.room >= 0 ? sim.campaignLevel.rooms[c.room].id : "?"; }
                        if (hurt < 0 && sim.health.hp < hpBefore && able >= 0) hurt = st;
                        if (able >= 0 && hurt >= 0) break;
                        continue;
                    }
                    // retry: the first wave in, then a death; then the first able enemy after the retry
                    if (killed < 0 && c.Wave33 > 0 && c.room >= 0 && sim.campaignLevel.rooms[c.room].kind != "boss" && sim.health.hp > 0)
                    {
                        killed = st;
                        sim.health.Damage(sim.health.max * 10, "start40 retry");
                        continue;
                    }
                    if (retryAt >= 0 && ableAfter < 0 && any) { ableAfter = st; ableKind = kind; break; }
                }
                string T(int s, int from) => s < 0 || from < 0 ? "never" : ((s - from) * sim.step).ToString("0.00", CultureInfo.InvariantCulture) + " s";
                if (!retry)
                    Console.WriteLine($"  L{li + 1} {id,-12} {pilot,-4}: first fighting room {enteredId}{(k > 0 ? " (put at its spawn)" : " (the level's spawn)")}: entry -> first enemy able to attack {T(able, live),-7} ({ableKind} in {firstRoom}); first spawn {T(spawned, live)}; first damage {T(hurt, live)}; hold {(sim.start40Holds > 0 ? release + " after " + sim.start40LastHold.ToString("0.00", CultureInfo.InvariantCulture) + " s" : "none")}, windowed {sim.start40Windowed}");
                else
                    Console.WriteLine($"  L{li + 1} {id,-12} {pilot,-4}: killed at {T(killed, live)}; retry in {retryRoom} -> first enemy able to attack {T(ableAfter, retryAt)} ({ableKind}); holds {sim.start40Holds} (last {release}, {sim.start40LastHold:0.00} s)");
                ap?.Detach();
            }
        }
        return 0;
    }
}
