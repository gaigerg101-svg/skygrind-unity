// NIGHT38 builder H (hud38 + juice38 are view rows: nothing here changes the sim). One entry, N38Hud (declared and called in Program.cs).
//   n38hud [cues|config] [--secs S]
//     config  design.n38.juice38's numbers read through GameConfig (the row's keys the view reads, the move specs, feel.trauma.boss and its events).
//     cues    the sim events juice38 listens to, from the REAL sim per boss: THE WICK, THE STAG, THE LOOM, THE CROWN (each from its NIGHT37 harness
//             set-up and BossRoomBegin) and THE KILN WYRM (its rig, kit.Start("wyrm")), the player standing still for S s (default 30; a death is
//             reset in place). Per boss: bossRise (the entrance: count and times; two inside entranceGap are one entrance), bossIntro (the card's
//             subtitle), the big-move events by stage, the wyrm's dig / erupt tells, the hurts taken (count, smallest / largest). PASS: every boss
//             raises its entrance (>= 1 bossRise) and no two entrances closer than entranceGap are counted twice (the view's guard covers them).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Hud(string cmd, string[] args, ref int code)
    {
        if (cmd != "n38hud") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        double secs = ArgInt36(args, "--secs", 30);
        int fails = 0;
        if (what == "config" || what == "all") fails += Juice38Config();
        if (what == "cues" || what == "all") fails += Juice38Cues(secs);
        Console.WriteLine(fails == 0 ? "n38hud: GREEN" : $"n38hud: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static int Juice38Config()
    {
        GameConfig cfg = Config();
        const string p = "design.n38.juice38.";
        string[] nums = { "entranceShake", "entranceHitstop", "entranceFlicker", "entranceGap", "moveShake", "moveFlicker", "shakeDecay", "hurtShakePer", "hurtShakeMin", "hurtShakeMax", "hurtStaticMin", "hurtStaticMax", "hurtHeavyAt", "staticDecay" };
        int fails = 0;
        foreach (string k in nums)
        {
            if (!cfg.Has(p + k)) { Console.WriteLine($"  config: {p}{k} MISSING"); fails++; continue; }
            Console.WriteLine($"  config: {k} = {cfg.Num(p + k)}");
        }
        Console.WriteLine($"  config: moves = [{string.Join(", ", cfg.Arr(p + "moves"))}], wyrmTells = [{string.Join(", ", cfg.Arr(p + "wyrmTells"))}]");
        Console.WriteLine($"  config: feel.trauma.boss = {cfg.Num("feel.trauma.boss")} on [{string.Join(", ", cfg.Arr("feel.trauma.bossEvents"))}] (cap {cfg.Num("feel.trauma.cap")})");
        Console.WriteLine($"  config: hud38.enabled = {cfg.Design("n38.hud38.enabled")}, juice38.enabled = {cfg.Design("n38.juice38.enabled")} (--off38 forces both off in the view: N38Blocks.ForceOff = {N38Blocks.ForceOff})");
        if (cfg.Num(p + "hurtStaticMax") < cfg.Num(p + "hurtStaticMin") || cfg.Num(p + "hurtShakeMax") < cfg.Num(p + "hurtShakeMin")) { Console.WriteLine("  config: RED: a max under its min"); fails++; }
        return fails;
    }

    sealed class Cue38
    {
        public string boss;
        public readonly List<double> rises = new List<double>();
        public int intros;
        public string subtitle = "";
        public readonly Dictionary<string, int> moves = new Dictionary<string, int>();
        public readonly Dictionary<string, int> tells = new Dictionary<string, int>();
        public int hurts;
        public string riseName = "", introName = "";
        public double hurtMin = double.MaxValue, hurtMax;
        public int deaths;
    }

    static string Stage38(object payload)
    {
        switch (payload)
        {
            case Wick37Fx w: return w.stage;
            case Stag37Event s: return s.stage;
            case Loom37Event l: return l.stage;
            case Crown37Event c: return c.stage;
            default: return "";
        }
    }

    /// <summary>The juice38 listeners' events on a bus, recorded against a clock (s).</summary>
    static Cue38 Listen38(string boss, EventBus events, GameConfig cfg, Func<double> now)
    {
        var r = new Cue38 { boss = boss };
        events.On<BossEvent>("bossRise", e => { r.rises.Add(now()); if (e != null) r.riseName = e.name ?? ""; });
        events.On<BossEvent>("bossIntro", e => { r.intros++; if (e != null) r.introName = e.name ?? ""; if (e != null && !string.IsNullOrEmpty(e.subtitle)) r.subtitle = e.subtitle; });
        var tells = new HashSet<string>(cfg.Arr("design.n38.juice38.wyrmTells").Select(o => (string)o));
        events.On<BossEvent>("bossTell", e =>
        {
            if (e == null || e.boss != "wyrm" || e.name == null || !tells.Contains(e.name)) return;
            r.tells[e.name] = (r.tells.TryGetValue(e.name, out int n) ? n : 0) + 1;
        });
        foreach (object o in cfg.Arr("design.n38.juice38.moves"))
        {
            string spec = (string)o;
            int c = spec.IndexOf(':');
            string name = c < 0 ? spec : spec.Substring(0, c), want = c < 0 ? null : spec.Substring(c + 1);
            events.On(name, payload =>
            {
                string st = Stage38(payload);
                string key = name + ":" + st + (want == null || want == st ? " (a move)" : " (not the move)");
                r.moves[key] = (r.moves.TryGetValue(key, out int n) ? n : 0) + 1;
            });
        }
        events.On<HurtEvent>("hurt", e =>
        {
            if (e == null || e.owner != "player" || !(e.amount > 0)) return;
            r.hurts++;
            r.hurtMin = Math.Min(r.hurtMin, e.amount);
            r.hurtMax = Math.Max(r.hurtMax, e.amount);
        });
        return r;
    }

    /// <summary>The player stands still for `secs` s (a death is reset in place: health full, Playing).</summary>
    static void Idle38(SurvivalSim sim, double secs, Cue38 r, ref int step)
    {
        var si = (ScriptedInput)sim.input;
        int n = MovementMath.StepsFor(secs, sim.cfg.Dt);
        for (int i = 0; i < n; i++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            step++;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
            }
        }
    }

    static int Juice38Cues(double secs)
    {
        GameConfig cfg0 = Config();
        double gap = cfg0.Num("design.n38.juice38.entranceGap");
        var all = new List<Cue38>();
        bool on37 = !N37Blocks.ForceOff;

        // THE WICK (level 1): the stack's tunnel mouth, BossRoomBegin("wick")
        if (on37)
        {
            SurvivalSim sim = Sim37W(1, true);
            int step = 0;
            Cue38 r = Listen38("THE WICK", sim.events, sim.cfg, () => step * sim.cfg.Dt);
            sim.Respawn(Mouth37(sim), 0);
            sim.Step();
            sim.BossRoomBegin("wick", out _, out _);
            Idle38(sim, secs, r, ref step);
            all.Add(r);
        }
        // THE STAG (level 2): the canal's gap, BossRoomBegin("stag37")
        if (on37)
        {
            SurvivalSim sim = Sim37S(1, true);
            int step = 0;
            Cue38 r = Listen38("THE STAG", sim.events, sim.cfg, () => step * sim.cfg.Dt);
            sim.Respawn(StagGap37(sim), 0);
            sim.Step();
            sim.BossRoomBegin("stag37", out _, out _);
            Idle38(sim, secs, r, ref step);
            all.Add(r);
        }
        // THE LOOM (level 4): the canyon's start ledge, BossRoomBegin("loom37")
        if (on37)
        {
            SurvivalSim sim = Loom37Sim(1, true);
            int step = 0;
            Cue38 r = Listen38("THE LOOM", sim.events, sim.cfg, () => step * sim.cfg.Dt);
            var start = sim.loom37Ledges.Find(l => l.id == "start");
            sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
            for (int i = 0; i < 10; i++) sim.Step();
            if (!sim.loadout.owned.Contains("nailgun")) sim.loadout.AddWeapon("nailgun");
            sim.BossRoomBegin("loom37", out _, out _);
            Idle38(sim, secs, r, ref step);
            all.Add(r);
        }
        // THE CROWN (level 5): the bridge's far half, BossRoomBegin("crown37")
        if (on37)
        {
            SurvivalSim sim = Sim37C(1, true);
            int step = 0;
            Cue38 r = Listen38("THE CROWN", sim.events, sim.cfg, () => step * sim.cfg.Dt);
            sim.Respawn(new Vec3(-10, 0, -268), 0);
            sim.Step();
            sim.BossRoomBegin("crown37", out _, out _);
            Idle38(sim, secs, r, ref step);
            all.Add(r);
        }
        // THE KILN WYRM (level 3): its rig over its arena (kit.Start("wyrm"): bossRise and bossIntro in the constructor), the player still at the spawn
        {
            GameConfig cfg = Config();
            string repo = Path.GetFullPath(Path.Combine(Root, ".."));
            var bossJson = Wyrm.LoadBoss(Path.Combine(repo, "Design", "bosses", "wyrm.json"));
            WyrmArena arena = WyrmArena.Load(Path.Combine(repo, "Design", "maps", "wyrm-arena.json"));
            // the rig's constructor starts the fight at once: listen on a bus the rig is given? It makes its own; so count from the rig's bus after
            // construction and add the start's own bossRise / bossIntro from its Emitted table (the constructor's emits)
            var rig = new WyrmRig(cfg, bossJson, arena, 7, 1e9);
            Cue38 r = Listen38("THE KILN WYRM", rig.events, cfg, () => rig.step * cfg.Dt);
            if (rig.events.Emitted.TryGetValue("bossRise", out int rises0)) for (int i = 0; i < rises0; i++) r.rises.Add(0);
            if (rig.events.Emitted.TryGetValue("bossIntro", out int intros0)) { r.intros += intros0; r.introName = rig.kit.NameOf("wyrm"); }
            var idle = new InputFrame();
            int n = MovementMath.StepsFor(secs, cfg.Dt);
            for (int i = 0; i < n; i++) rig.Step(idle, false);
            all.Add(r);
        }

        int fails = 0;
        foreach (Cue38 r in all)
        {
            int entrances = 0;
            double last = double.NegativeInfinity;
            foreach (double t in r.rises) { if (t - last >= gap) entrances++; last = t; }
            string rises = string.Join(", ", r.rises.Select(t => t.ToString("0.00") + " s"));
            string moves = r.moves.Count == 0 ? "none" : string.Join(", ", r.moves.Select(kv => $"{kv.Key} x{kv.Value}"));
            string tells = r.tells.Count == 0 ? "" : " ; wyrm dig / erupt tells: " + string.Join(", ", r.tells.Select(kv => $"{kv.Key} x{kv.Value}"));
            string hurts = r.hurts == 0 ? "none" : $"{r.hurts} (damage {r.hurtMin:0.#} .. {r.hurtMax:0.#}), deaths reset {r.deaths}";
            bool ok = entrances >= 1;
            if (!ok) fails++;
            Console.WriteLine($"  {r.boss}: bossRise x{r.rises.Count} at [{rises}] = {entrances} entrance(s) after the {gap} s guard, card name '{r.riseName}'; bossIntro x{r.intros} '{r.introName}'{(r.subtitle.Length > 0 ? " ('" + r.subtitle + "')" : "")}; moves: {moves}{tells}; hurts: {hurts}: {(ok ? "PASS" : "RED")}");
        }
        if (!on37) Console.WriteLine("  --off37 / older: the NIGHT37 bosses are off (only THE KILN WYRM is checked)");
        return fails;
    }
}
