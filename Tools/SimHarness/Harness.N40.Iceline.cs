// NIGHT40 builder L2 (NIGHT40.md card L2): n40-iceline (level 2 rebuilt for the gauntlet), headless. One entry, N40Iceline (declared and called in Program.cs).
//   n40ice level                 boots n40-iceline in campaign mode with iceline40 on: the rooms in order, the owned weapons at the start (the gauntlet alone) and
//                                after entering g6 THE ARMOURY (+ the KESTREL), each wave's kinds placed (warden / drone / charger spawn), x9 the boss room.
//   n40ice walk [--seeds N] [--from <roomId>] [--upto <roomId>] [--cap s] [--nofoes] [--trace]
//                                the walk bot (Walk29Pilot + Walk40Iceline through Walk40IcelineInput) through the playable rooms in order, iceline40 on; per room
//                                cleared k / n, median s, deaths, falls; --upto stops a run once that room is cleared (round 1: --upto g3).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    const string Ice40Path = "Design/levels/n40-iceline.json";

    static partial void N40Iceline(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40ice") return;
        Ice40Foes = Array.IndexOf(args, "--nofoes") < 0; // foes40 (E3) on unless --nofoes
        Walk29Pilot.Ice40NoBlast = Array.IndexOf(args, "--noblast") >= 0;
        Ice40Brawl = Array.IndexOf(args, "--brawl") >= 0; // brawl40 (B2) off unless --brawl (the walk stops at x9's door)
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "level";
        int fails = 0;
        if (what == "level") fails += Ice40Level();
        if (what == "prompts") fails += Ice40Prompts();
        if (what == "walk")
        {
            int seeds = 5;
            int si = Array.IndexOf(args, "--seeds");
            if (si >= 0 && si + 1 < args.Length) seeds = int.Parse(args[si + 1]);
            int ui = Array.IndexOf(args, "--upto");
            string upto = ui >= 0 && ui + 1 < args.Length ? args[ui + 1] : null;
            int fi = Array.IndexOf(args, "--from");
            string from = fi >= 0 && fi + 1 < args.Length ? args[fi + 1] : null;
            int ci = Array.IndexOf(args, "--cap");
            double cap = ci >= 0 && ci + 1 < args.Length ? double.Parse(args[ci + 1], System.Globalization.CultureInfo.InvariantCulture) : 1800;
            fails += Ice40Walk(seeds, upto, Array.IndexOf(args, "--trace") >= 0, from, cap);
        }
        Console.WriteLine(fails == 0 ? "n40ice: GREEN" : $"n40ice: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static string P40I(bool ok) => ok ? "PASS" : "FAIL";
    static bool Ice40Foes = true, Ice40Brawl;

    static SurvivalSim Ice40Sim(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Ice40Path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n40Override = new N40Blocks { iceline40 = true, foes40 = Ice40Foes, brawl40 = Ice40Brawl }; // round 2: foes40 on by default (E3: the gauntlet parries only with it; g4 THE REACH needs it)
        return sim;
    }

    /// <summary>level: the rooms, the start set (the gauntlet alone), the KESTREL after g6's entry, every wave's kinds placed (each room's waves spawned
    /// at once on its nodes, the player at the room's spawn), the boss room x9.</summary>
    static int Ice40Level()
    {
        SurvivalSim sim = Ice40Sim(1);
        sim.StartRun(1, null, null);
        var si = (ScriptedInput)sim.input;
        for (int k = 0; k < 4; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); } // the unlock rule (N31UnlocksStep) runs in the step
        var ids = sim.campaignLevel.rooms.ConvertAll(r => r.id);
        string start = string.Join(",", sim.loadout.owned);
        bool startOk = sim.loadout.owned.Count == 1 && sim.loadout.owned.Contains("gauntlet");
        Console.WriteLine($"n40-iceline: {ids.Count} rooms [{string.Join(" ", ids)}]; owned at the start [{start}] (the gauntlet alone): {P40I(startOk)}");
        int fails = startOk ? 0 : 1;
        // walk into g6 by respawning at its spawn and stepping: the room award gives the KESTREL on entry
        RoomDef g6 = sim.campaignLevel.rooms.Find(r => r.id == "g6");
        sim.husks.frozen = true;
        if (sim.campaign == null) { Console.WriteLine("  no campaign after 4 steps (FAIL)"); return fails + 1; }
        // the campaign's room entry (Campaign24.Enter is private: the harness calls it by reflection, as the door would), the player at g6's spawn
        typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { sim.campaignLevel.rooms.IndexOf(g6) });
        sim.Respawn(g6.spawn, 0);
        for (int k = 0; k < 30; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
        string inG6 = sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "?";
        bool kestrel = sim.loadout.owned.Contains("revolver");
        Console.WriteLine($"  in room {inG6}: owned [{string.Join(",", sim.loadout.owned)}]: the KESTREL given on entering g6: {P40I(kestrel && inG6 == "g6")}");
        if (!(kestrel && inG6 == "g6")) fails++;
        // every wave's kinds: a fresh sim per room, each kind spawned at the room's first node (the world's Spawn, as the campaign's)
        var kinds = new SortedSet<string>();
        foreach (RoomDef r in sim.campaignLevel.rooms) foreach (var w in r.waves) foreach (string k in w) kinds.Add(k);
        foreach (string k in kinds)
        {
            SurvivalSim s2 = Ice40Sim(1);
            s2.StartRun(1, null, null);
            RoomDef r = s2.campaignLevel.rooms.Find(q => q.waves.Exists(w => w.Contains(k)));
            Vec3 at = r.nodes[0];
            Husk h = s2.husks.Spawn(k, at, r.spawn);
            bool ok = h != null && h.alive && h.kind == (k == "tank" ? "husk" : k);
            Console.WriteLine($"  kind {k}: spawned in {r.id} at ({at.x:0.0}, {at.y:0.0}, {at.z:0.0}) as '{h?.kind}' hp {h?.hp:0}: {P40I(ok)}");
            if (!ok) fails++;
        }
        RoomDef boss = sim.campaignLevel.rooms.Find(r => r.kind == "boss");
        bool bossOk = boss != null && boss.id == "x9" && Math.Abs(boss.x1 - boss.x0 - 38) < 1e-9 && Math.Abs(boss.z1 - boss.z0 - 38) < 1e-9;
        Console.WriteLine($"  boss room {boss?.id} '{boss?.name}' ({boss?.boss}): {boss?.x1 - boss?.x0:0} x {boss?.z1 - boss?.z0:0} m: {P40I(bossOk)}");
        if (!bossOk) fails++;
        return fails;
    }

    /// <summary>prompts (round 3): every lesson line of n40-iceline as Lesson25TextTests checks it, headless: each {placeholder} an action on the v5 pad
    /// layout with a button and a keyboard binding (fire / altFire: the mouse), at most 6 words, and its keyboard text (the key's code name, the mouse
    /// buttons spelled out) at most 45 characters (the lead's cut: teach35's box wraps at ~52).</summary>
    static int Ice40Prompts()
    {
        GameConfig cfg = Config();
        var bindings = new Bindings(cfg);
        PadLayout v5 = GamepadMath.DefaultForVersion(5);
        int fails = 0;
        foreach (object ro in PortData.Arr(PortData.Obj(PortData.Obj(PortData.LoadJson(Ice40Path))["campaign"])["rooms"]))
        {
            var rd = PortData.Obj(ro);
            if (!rd.TryGetValue("lessons", out object lo)) continue;
            foreach (object l in PortData.Arr(lo))
            {
                string raw = PortData.Obj(l)["prompt"] as string ?? "";
                var bad = new List<string>();
                string kb = System.Text.RegularExpressions.Regex.Replace(raw, "\\{([a-zA-Z]+)\\}", m =>
                {
                    string a = m.Groups[1].Value;
                    if (a == "fire") return "LEFT CLICK";
                    if (a == "altFire") return "RIGHT CLICK";
                    int act = GamepadMath.ActionFromName(a);
                    if (act < 0 || !GamepadMath.ButtonFor((PadAction)act, v5).HasValue) bad.Add($"{{{a}}} not on the v5 pad");
                    string code = bindings.Code(a);
                    if (code == null) { bad.Add($"{{{a}}} has no key"); return "?"; }
                    return code.Replace("Key", "").Replace("Left", "").ToUpperInvariant();
                });
                int words = System.Text.RegularExpressions.Regex.Replace(raw, "\\{[a-zA-Z]+\\}", "X").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (words > 6) bad.Add($"{words} words");
                if (kb.Length > 45) bad.Add($"{kb.Length} chars");
                Console.WriteLine($"  {rd["id"]}: '{raw}' -> '{kb}' ({words} words, {kb.Length} chars): {(bad.Count == 0 ? "PASS" : "FAIL " + string.Join("; ", bad))}");
                if (bad.Count > 0) fails++;
            }
        }
        return fails;
    }

    /// <summary>walk: the playable rooms in order through the real sim (walk29's loop: locked doors, enemies at the nodes, falls), iceline40 on.</summary>
    static int Ice40Walk(int seeds, string upto, bool trace, string fromId = null, double capS = 1800)
    {
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Ice40Path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        if (upto != null) { int u = level.rooms.FindIndex(r => r.id == upto); if (u >= 0) playable = u + 1; }
        int from = fromId == null ? 0 : Math.Max(0, level.rooms.FindIndex(r => r.id == fromId));
        var cleared = new int[playable];
        var deaths = new int[playable];
        var falls = new int[playable];
        var times = new List<double>[playable];
        for (int i = 0; i < playable; i++) times[i] = new List<double>();
        int done = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalSim sim = Ice40Sim(seed);
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
            var input = new Walk40IcelineInput(new Walk29Pilot(sim, ap));
            sim.UseInput(input);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { cleared[e.room]++; times[e.room].Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) deaths[e.room]++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) falls[e.room]++; });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s; owned [{string.Join(",", sim.loadout.owned)}]"));
            if (trace) sim.events.On<RoomEvent>("roomRetry", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} RETRY (died)"));
            sim.StartRun(seed, null, null);
            int need = playable - from;
            if (from > 0)
            {
                // --from: one step (the campaign starts), the first room's enemies dropped, the campaign's entry into `from` (Campaign24.Enter is private:
                // by reflection, as a door would), the player at that room's spawn. The pips start empty (no earlier kills), the start set as the level's.
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                sim.husks.KillAll();
                sim.brain.Clear();
                typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { from });
                sim.Respawn(level.rooms[from].spawn, 0);
                // past g6 the level has given the KESTREL (iceline40.awards): the same here
                if (from > level.rooms.FindIndex(r => r.id == "g6") && !sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
            }
            int max = MovementMath.StepsFor(capS, sim.step), st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (s == 0 && st == 3) Console.WriteLine($"  owned after the first steps: [{string.Join(",", sim.loadout.owned)}]"); // the unlock rule runs in the step
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= need) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= need;
            if (ok) done++;
            else
            {
                Vec3 p = sim.mover.position;
                var left = new List<string>();
                foreach (string k in new[] { "husk", "swarmer", "spitter", "charger", "drone", "warden" })
                    foreach (Husk h in sim.husks.PoolOf(k)) if (h.alive && left.Count < 6) left.Add($"{k}({h.position.x:0.0},{h.position.y:0.0},{h.position.z:0.0})");
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")} {sim.campaign?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount} [{string.Join(" ", left)}], unsticks {input.pilot.unsticks}, hp {sim.health.hp:0}");
            }
            ap.Detach();
        }
        for (int i = from; i < playable; i++)
        {
            times[i].Sort();
            double med = times[i].Count == 0 ? double.NaN : times[i][times[i].Count / 2];
            Console.WriteLine($"walk40 iceline {level.rooms[i].id} {level.rooms[i].name}: cleared {cleared[i]}/{seeds}, median {med:0.0} s (p90 {(times[i].Count == 0 ? double.NaN : times[i][Math.Min(times[i].Count - 1, (int)(times[i].Count * 0.9))]):0.0}), deaths {deaths[i]}, falls {falls[i]}");
        }
        Console.WriteLine($"walk40 iceline SUMMARY (foes40 {(Ice40Foes ? "on" : "off")}): rooms {from + 1}-{playable} ({level.rooms[from].id}..{level.rooms[playable - 1].id}) cleared in {done} of {seeds} runs: {P40I(done == seeds)}");
        return done == seeds ? 0 : 1;
    }
}
