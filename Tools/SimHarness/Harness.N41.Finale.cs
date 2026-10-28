// NIGHT41 builder L6 (NIGHT41.md card L6; DECIDE41 D-3): n41-finale, the level 6 GREYBOX (Design/levels/n41/finale.beats.md; Tools/night/gen-n41-finale.cjs),
// headless. One entry, N41Finale (declared and called in Program.cs). design.n41.finale41 stays OFF: these commands load the level file directly, in campaign
// mode, and change no config.
//   n41fin level                 the rooms in order (ids, sizes, waves), each wave's kinds spawned on the room's first node, the boss room mx (kind boss, id
//                                "rival41": no sim code answers it, so entering it clears at once: measured here).
//   n41fin walk [--seeds N] [--upto <roomId>] [--cap s] [--trace]
//                                the walk bot (Walk29Pilot + the ported autopilot) through the playable rooms m1..mr in order; per room cleared k / n, median s,
//                                deaths, falls. The weapons levels 1-3 award (the KESTREL, the PULSE gauntlet, the THUNDERCLAP) are given at the start by
//                                hand: with finale41 off the curriculum does not place n41-finale, so no unlock row gives them (the fist is always there).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    const string Fin41Path = "Design/levels/n41-finale.json";
    static string[] Fin41Weapons = { "revolver", "gauntlet", "shotgun" }; // levels 1-3's weapons (VERBS rows 25-30); the NAILGUN is out (loom40), the TORCH not counted on

    static partial void N41Finale(string cmd, string[] args, ref int code)
    {
        if (cmd != "n41fin") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "level";
        int fails = 0;
        if (what == "level") fails += Fin41Level();
        if (what == "walk")
        {
            int seeds = 5;
            int si = Array.IndexOf(args, "--seeds");
            if (si >= 0 && si + 1 < args.Length) seeds = int.Parse(args[si + 1]);
            int ui = Array.IndexOf(args, "--upto");
            string upto = ui >= 0 && ui + 1 < args.Length ? args[ui + 1] : null;
            int ci = Array.IndexOf(args, "--cap");
            double cap = ci >= 0 && ci + 1 < args.Length ? double.Parse(args[ci + 1], System.Globalization.CultureInfo.InvariantCulture) : 1800;
            int pi = Array.IndexOf(args, "--probe");
            Fin41Probe = pi >= 0 && pi + 1 < args.Length ? args[pi + 1] : null; // --probe <roomId>: one line a second in that room
            int ai = Array.IndexOf(args, "--arm");
            if (ai >= 0 && ai + 1 < args.Length) Fin41Weapons = args[ai + 1].Split(','); // --arm a,b: the weapons given at the start (a control)
            int fi = Array.IndexOf(args, "--from");
            fails += Fin41Walk(seeds, upto, Array.IndexOf(args, "--trace") >= 0, cap, fi >= 0 && fi + 1 < args.Length ? args[fi + 1] : null);
        }
        Console.WriteLine(fails == 0 ? "n41fin: GREEN" : $"n41fin: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static string P41F(bool ok) => ok ? "PASS" : "FAIL";
    static string Fin41Probe;

    static SurvivalSim Fin41Sim()
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Fin41Path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        return sim;
    }

    static void Fin41Arm(SurvivalSim sim)
    {
        foreach (string w in Fin41Weapons) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
    }

    /// <summary>level: the rooms, every wave's kinds placed, the empty boss room mx clearing at once on entry.</summary>
    static int Fin41Level()
    {
        SurvivalSim sim = Fin41Sim();
        sim.StartRun(1, null, null);
        var si = (ScriptedInput)sim.input;
        for (int k = 0; k < 4; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
        int fails = 0;
        if (sim.campaign == null) { Console.WriteLine("  no campaign after 4 steps (FAIL)"); return 1; }
        foreach (RoomDef r in sim.campaignLevel.rooms)
            Console.WriteLine($"  {r.id} '{r.name}' ({r.kind}): {r.x1 - r.x0:0} x {r.z1 - r.z0:0} m, waves [{string.Join(" | ", r.waves.ConvertAll(w => string.Join(",", w)))}]");
        var kinds = new SortedSet<string>();
        foreach (RoomDef r in sim.campaignLevel.rooms) foreach (var w in r.waves) foreach (string k in w) kinds.Add(k);
        foreach (string k in kinds)
        {
            SurvivalSim s2 = Fin41Sim();
            s2.StartRun(1, null, null);
            RoomDef r = s2.campaignLevel.rooms.Find(q => q.waves.Exists(w => w.Contains(k)));
            Vec3 at = r.nodes[0];
            Husk h = s2.husks.Spawn(k, at, r.spawn);
            bool ok = h != null && h.alive && h.kind == (k == "tank" ? "husk" : k);
            Console.WriteLine($"  kind {k}: spawned in {r.id} at ({at.x:0.0}, {at.y:0.0}, {at.z:0.0}) as '{h?.kind}' hp {h?.hp:0}: {P41F(ok)}");
            if (!ok) fails++;
        }
        // mx: the campaign's entry (Campaign24.Enter is private: by reflection, as a door would), the player at its spawn; it must clear at once with no boss
        RoomDef mx = sim.campaignLevel.rooms.Find(r => r.id == "mx");
        sim.husks.frozen = true;
        typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { sim.campaignLevel.rooms.IndexOf(mx) });
        sim.Respawn(mx.spawn, 0);
        for (int k = 0; k < 30; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
        bool mxOk = mx.kind == "boss" && mx.boss == "rival41" && sim.campaign.phase != Campaign24.Phase.Fight && sim.enemies.AliveCount == 0;
        Console.WriteLine($"  boss room mx '{mx.name}' (kind {mx.kind}, boss '{mx.boss}'): {mx.x1 - mx.x0:0} x {mx.z1 - mx.z0:0} m; after 30 steps phase {sim.campaign.phase}, alive {sim.enemies.AliveCount}, flow {sim.flow} (empty: clears at once): {P41F(mxOk)}");
        if (!mxOk) fails++;
        return fails;
    }

    /// <summary>walk: the playable rooms in order through the real sim (walk29's loop: locked doors, enemies at the nodes, falls).</summary>
    static int Fin41Walk(int seeds, string upto, bool trace, double capS, string fromId = null)
    {
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Fin41Path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        if (upto != null) { int u = level.rooms.FindIndex(r => r.id == upto); if (u >= 0) playable = u + 1; }
        var cleared = new int[playable];
        var deaths = new int[playable];
        var falls = new int[playable];
        var times = new List<double>[playable];
        for (int i = 0; i < playable; i++) times[i] = new List<double>();
        int from = fromId == null ? 0 : Math.Max(0, level.rooms.FindIndex(r => r.id == fromId));
        int done = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalSim sim = Fin41Sim();
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
            sim.UseInput(pilot);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { cleared[e.room]++; times[e.room].Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) deaths[e.room]++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) falls[e.room]++; });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s; hp {sim.health.hp:0}"));
            if (trace) sim.events.On<RoomEvent>("roomRetry", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} RETRY (died)"));
            sim.StartRun(seed, null, null);
            Fin41Arm(sim);
            if (from > 0)
            {
                // --from: one step (the campaign starts), the first room's enemies dropped, the campaign's entry into `from` (Campaign24.Enter is private:
                // by reflection, as a door would; as n40ice walk --from), the player at that room's spawn
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                sim.husks.KillAll();
                sim.brain.Clear();
                typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { from });
                sim.Respawn(level.rooms[from].spawn, 0);
            }
            int need = playable - from;
            int max = MovementMath.StepsFor(capS, sim.step), st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (st == 3) Fin41Arm(sim); // the unlock rule runs in the step: arm again after it
                if (s == 0 && st == 4) Console.WriteLine($"  owned after the first steps: [{string.Join(",", sim.loadout.owned)}]");
                if (Fin41Probe != null && sim.campaign != null && sim.campaign.room >= 0 && level.rooms[sim.campaign.room].id == Fin41Probe && st % MovementMath.StepsFor(1.0, sim.step) == 0)
                {
                    Vec3 p = sim.mover.position;
                    var near = new List<string>();
                    foreach (string k in new[] { "husk", "swarmer", "spitter", "charger", "drone", "warden" })
                        foreach (Husk h in sim.husks.PoolOf(k)) if (h.alive && near.Count < 8) near.Add($"{k}({h.position.x:0.0},{h.position.y:0.0},{h.position.z:0.0})");
                    Console.WriteLine($"  [probe {st * sim.step:0} s] at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}) hp {sim.health.hp:0} hand {sim.loadout.Weapon} phase {sim.campaign.phase} alive {sim.enemies.AliveCount} [{string.Join(" ", near)}]");
                }
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
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")} {sim.campaign?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount} [{string.Join(" ", left)}], unsticks {pilot.unsticks}, hp {sim.health.hp:0}");
            }
            ap.Detach();
        }
        for (int i = from; i < playable; i++)
        {
            times[i].Sort();
            double med = times[i].Count == 0 ? double.NaN : times[i][times[i].Count / 2];
            Console.WriteLine($"walk41 finale {level.rooms[i].id} {level.rooms[i].name}: cleared {cleared[i]}/{seeds}, median {med:0.0} s (p90 {(times[i].Count == 0 ? double.NaN : times[i][Math.Min(times[i].Count - 1, (int)(times[i].Count * 0.9))]):0.0}), deaths {deaths[i]}, falls {falls[i]}");
        }
        Console.WriteLine($"walk41 finale SUMMARY: rooms {from + 1}-{playable} ({level.rooms[from].id}..{level.rooms[playable - 1].id}) cleared in {done} of {seeds} runs: {P41F(done == seeds)}");
        return done == seeds ? 0 : 1;
    }
}
