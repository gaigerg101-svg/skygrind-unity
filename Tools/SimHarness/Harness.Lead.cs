// NIGHT14 (the lead's harness commands). U3-27.2: where a second run on one composition leaves the first's path.
//   slate        two seed-1 fingerprint runs on one composition; each system hashed on its own right after StartRun and
//                after every step; prints the first point where a system differs, and which systems differ there.
using System;
using System.Collections.Generic;
using System.Reflection;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Lead(string cmd, string[] args, ref int code)
    {
        if (Night15(cmd, args, ref code)) return; // NIGHT15's commands (Harness.Night15.cs)
        if (cmd != "slate") return;
        SurvivalSim sim = SurvivalSim.Headless(Config(), Map());
        var runs = new List<List<(string, ulong)[]>>();
        List<(string, ulong)[]> current = null;
        sim.RunStarted += () => current.Add(SlateSections(sim));
        sim.StepEnded += () => current.Add(SlateSections(sim));
        for (int r = 0; r < 2; r++)
        {
            current = new List<(string, ulong)[]>();
            runs.Add(current);
            sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out _);
        }
        List<(string, ulong)[]> a = runs[0], b = runs[1];
        for (int i = 0; i < Math.Min(a.Count, b.Count); i++)
        {
            var differ = new List<string>();
            for (int s = 0; s < a[i].Length; s++)
                if (a[i][s].Item2 != b[i][s].Item2) differ.Add(a[i][s].Item1);
            if (differ.Count == 0) continue;
            Console.WriteLine($"slate: first difference {(i == 0 ? "right after StartRun" : $"after step {i}")}: {string.Join(", ", differ)}");
            code = 1;
            return;
        }
        Console.WriteLine($"slate: no section differs over {a.Count} points");
        code = 0;
    }

    static uint SlateRng(object owner, string field)
    {
        if (owner == null) return 0;
        FieldInfo f = owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var rng = f?.GetValue(owner) as Mulberry32;
        return rng == null ? 0 : rng.State;
    }

    /// <summary>SimFingerprint.Hash's state, one hash per system (and per stream), plus a few the fingerprint doesn't read.</summary>
    static (string, ulong)[] SlateSections(SurvivalSim g)
    {
        var list = new List<(string, ulong)>();
        SimFingerprint.Fnv f;
        f = SimFingerprint.Fnv.Start();
        f.V(g.mover.position); f.V(g.mover.velocity); f.I((int)g.mover.state); f.B(g.mover.grounded); f.B(g.mover.crouched);
        list.Add(("mover", f.h));
        f = SimFingerprint.Fnv.Start(); f.D(g.input.Yaw); f.D(g.input.Pitch); list.Add(("look", f.h));
        f = SimFingerprint.Fnv.Start(); f.D(g.health.hp); f.D(g.health.max); f.D(g.health.hard); list.Add(("health", f.h));
        f = SimFingerprint.Fnv.Start(); f.D(g.economy.total); f.D(g.economy.earned); f.D(g.economy.spent); list.Add(("economy", f.h));
        SurvivalRun run = g.run;
        f = SimFingerprint.Fnv.Start(); f.I((int)run.state); f.I(run.round); f.I(run.cleared); f.I(run.steps); f.I(run.phaseSteps); list.Add(("run", f.h));
        f = SimFingerprint.Fnv.Start(); f.I(run.stats.kills); f.I(run.stats.headshots); f.I(run.stats.shots); f.I(run.stats.shotsHit); list.Add(("stats", f.h));
        DirectorState ds = g.director.state;
        f = SimFingerprint.Fnv.Start(); f.D(ds.intensity); f.S(ds.phase); f.D(ds.phaseTime); f.D(ds.relaxTime); f.D(ds.camp); f.B(ds.hunted); f.B(ds.exposed); list.Add(("director", f.h));
        f = SimFingerprint.Fnv.Start();
        int alive = 0;
        foreach (Enemy e in g.enemies.all)
        {
            if (!e.alive) continue;
            alive++;
            f.S(e.kind); f.D(e.hp); f.V(e.position); f.V(e.velocity); f.D(e.yaw); f.I(e.windupSteps); f.I(e.staggerSteps); f.I(e.knockSteps);
        }
        f.I(alive);
        list.Add(("enemies", f.h));
        f = SimFingerprint.Fnv.Start();
        foreach (Enemy e in g.enemies.all) { f.I(e.id); f.B(e.alive); f.S(e.kind); }
        list.Add(("enemyIds", f.h));
        f = SimFingerprint.Fnv.Start(); f.D(g.style.core.meter); f.D(g.style.core.total); f.I(g.style.rank); f.I(g.style.core.bestRank); list.Add(("style", f.h));
        Loadout lo = g.loadout;
        f = SimFingerprint.Fnv.Start(); f.I(lo.owned.Count); foreach (string w in lo.owned) f.S(w); f.I(lo.slot); f.I(lo.targetSlot); f.I(lo.shotSeq); f.B(lo.ready); list.Add(("loadout", f.h));
        f = SimFingerprint.Fnv.Start(); for (int z = 0; z < g.map.zones.Count; z++) f.B(g.zones.IsLocked(z)); list.Add(("zones", f.h));
        f = SimFingerprint.Fnv.Start(); f.I(g.perks.owned.Count); foreach (string p in g.perks.owned) f.S(p); list.Add(("perks", f.h));
        f = SimFingerprint.Fnv.Start(); foreach (var kv in g.mods.stacks) { f.S(kv.Key); f.I(kv.Value); } list.Add(("mods", f.h));
        // simStep (the sim time since boot) is not compared: only time differences are read from it.
        (object, string, string)[] streams =
        {
            (g.loadout, "rng", "rng.loadout"), (g.projectiles, "rng", "rng.projectiles"), (g.director, "rng", "rng.director"),
            (g.director, "paceRng", "rng.directorPace"), (g.brain, "rng", "rng.brain"), (g.husks.acid, "rng", "rng.acid"),
            (g.powerUps, "killRng", "rng.powerUpsKill"), (g.powerUps, "rng", "rng.powerUps"), (g.mods, "draftRng", "rng.modsDraft"),
            (g.mods, "rollRng", "rng.modsRoll"), (g.forge, "rng", "rng.forge"), (g.box, "rng", "rng.box"), (g.exfil, "rng", "rng.exfil"),
        };
        foreach ((object o, string field, string name) in streams)
        {
            f = SimFingerprint.Fnv.Start();
            f.U64(SlateRng(o, field));
            list.Add((name, f.h));
        }
        return list.ToArray();
    }
}
