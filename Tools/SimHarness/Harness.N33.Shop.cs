// NIGHT33 P (DESIGN33 card P, D-6, row design.n33.shop33): campaign points that buy something. Dispatched by the lead's Program.cs:
// N33Shop(cmd, args, ref code).
//   shop33 earn [--seeds n] [--first n] [--cap s] [--levels foundry,iceline,kiln,span,spire]
//     MEASURE FIRST: walk29's pilot (the walk bots: Walk29Pilot + the autopilot's fights) plays each n32 level's playable rooms (the boss room excluded,
//     as walk29 does) through the REAL sim with the design config as it is; prints the points each run earned (Economy.earned) per level, the p50 per
//     level and the p50 of the level p50s (the "typical earn" the shop's prices are 1x / 2x / 4x of).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N33Shop(string cmd, string[] args, ref int code)
    {
        if (cmd != "shop33") return;
        code = 0;
        string sub = args.Length > 1 ? args[1] : "earn";
        if (sub == "earn") { Shop33Earn(args, ref code); return; }
        if (sub == "effects") { Shop33Effects(args, ref code); return; }
        if (sub == "tape") { Shop33Tape(args, ref code); return; }
        Console.WriteLine("shop33: earn [--seeds n] [--first n] [--cap s] [--levels a,b] | effects | tape [--seed n] [--seconds s]");
        code = 2;
    }

    /// <summary>Item i owned and equipped (Core/Shop33.cs's encoding).</summary>
    static int Shop33One(int i) => (1 << i) | (1 << (Shop33Catalogue.MaxItems + i));

    /// <summary>A survival composition with shop33 on (every other NIGHT33 block off) or off, `code` owned, the player invulnerable.</summary>
    static SurvivalSim Shop33Sim(GameConfig cfg, SurvivalMap map, int code, bool on, uint seed = 3301)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n33Override = new N33Blocks { shop33 = on };
        sim.shop33Owned = code;
        sim.bank33 = 0;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        sim.Respawn(map.playerSpawn, 0);
        return sim;
    }

    /// <summary>A gun's full charge through a line of bags (4, 6, 8 m ahead; the aim at the middle one): the charged shots fired and their hits, held
    /// for the hold + `fullSeconds` + 0.05 s, then released.</summary>
    static (int charged, int hits) Shop33Charge(GameConfig cfg, SurvivalMap map, int code, bool on, string gun, double fullSeconds, int bags)
    {
        SurvivalSim sim = Shop33Sim(cfg, map, code, on);
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 9);
        Vec3 sp = map.playerSpawn;
        int charged = 0, hits = 0;
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { charged++; hits += e.hits; });
        double dt = sim.step;
        int lead = MovementMath.StepsFor(0.5, dt);
        double holdS = sim.loadout.charge27 != null ? sim.loadout.charge27.p.holdSeconds : 0.2;
        int end = lead + MovementMath.StepsFor(holdS, dt) + MovementMath.StepsFor(fullSeconds, dt) + MovementMath.StepsFor(0.05, dt);
        var line = new Husk[bags];
        for (int s = 0; s < end + MovementMath.StepsFor(0.3, dt); s++)
        {
            for (int b = 0; b < bags; b++) line[b] = Pin27(sim, line[b], sp, ux, uz, 4 + 2 * b, 1e9);
            double py = 0;
            Husk aimAt = line[bags / 2];
            if (aimAt != null && aimAt.alive) (yaw, py) = ArclineLook(sim.Eye, aimAt.AimPoint);
            bool down = s >= lead && s < end, prev = s - 1 >= lead && s - 1 < end;
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && !prev } });
        }
        return (charged, hits);
    }

    /// <summary>1 s of STITCHER nails into a bag, then its SPECIAL (the detonate): the mean damage of the detonate's hits.</summary>
    static double Shop33Detonate(GameConfig cfg, SurvivalMap map, int code, bool on)
    {
        SurvivalSim sim = Shop33Sim(cfg, map, code, on);
        if (!sim.loadout.owned.Contains("nailgun")) sim.loadout.AddWeapon("nailgun");
        int slot = sim.loadout.owned.IndexOf("nailgun");
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 5);
        Vec3 sp = map.playerSpawn;
        double dt = sim.step;
        int nailEnd = MovementMath.StepsFor(1.0, dt), special = nailEnd + MovementMath.StepsFor(0.8, dt), end = special + MovementMath.StepsFor(0.3, dt);
        var hits = new List<double>();
        int s = 0;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "nailgun" && s >= special) hits.Add(e.damage); });
        Husk bag = null;
        for (s = 0; s < end; s++)
        {
            bag = Pin27(sim, bag, sp, ux, uz, 4.0, 1e9);
            double py = 0;
            if (bag != null && bag.alive) (yaw, py) = ArclineLook(sim.Eye, bag.AimPoint);
            bool nail = s < nailEnd, sp2 = s == special;
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = nail, pressed = s == 0 }, special = new Button { held = sp2, pressed = sp2 } });
        }
        return hits.Count > 0 ? hits.Sum() / hits.Count : 0;
    }

    /// <summary>One dash's worth of the bar spent (the boost's cost), then standing still: the steps until the bar is full again.</summary>
    static int Shop33DashRefill(GameConfig cfg, SurvivalMap map, int code, bool on)
    {
        SurvivalSim sim = Shop33Sim(cfg, map, code, on);
        Stamina.State st = sim.mover.stamina;
        if (!st.rules.Any) return -1;
        double max = st.bar;
        st.bar = max - st.rules.cost;
        st.sinceSpend = 0;
        st.full = false;
        sim.mover.stamina = st;
        for (int s = 1; s < MovementMath.StepsFor(10, sim.step); s++)
        {
            Tick27(sim, 0, 0, new CombatInput { weapon = -1 });
            if (sim.mover.stamina.bar >= max - 1e-9) return s;
        }
        return -1;
    }

    /// <summary>A bag with 1 hp 4 m ahead and the player at 50 hp: one KESTREL tap kills it; the player's hp after.</summary>
    static (double hp, int kills) Shop33KillHeal(GameConfig cfg, SurvivalMap map, int code, bool on)
    {
        SurvivalSim sim = Shop33Sim(cfg, map, code, on);
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        int slot = sim.loadout.owned.IndexOf("revolver");
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 5);
        Vec3 sp = map.playerSpawn;
        Husk bag = sim.husks.Spawn("husk", new Vec3(sp.x + ux * 4, sp.y, sp.z + uz * 4), sp);
        bag.hp = bag.maxHp = 1;
        sim.health.hp = 50;
        int kills = 0;
        sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; });
        double py = 0;
        for (int s = 0; s < MovementMath.StepsFor(0.4, sim.step); s++)
        {
            if (bag.alive) { bag.position = new Vec3(sp.x + ux * 4, sp.y, sp.z + uz * 4); (yaw, py) = ArclineLook(sim.Eye, bag.AimPoint); }
            bool f = s == 10;
            Tick25(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = f, pressed = f } });
        }
        return (sim.health.hp, kills);
    }

    /// <summary>`shop33 effects`: each upgrade ON (owned and equipped, row on) against OFF (the row off, the same code) in a short sim; then a run with
    /// every upgrade followed by a run with the row off on the SAME sim (nothing carries over). Exit 1 on a RED line.</summary>
    static void Shop33Effects(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        Shop33Catalogue cat = Shop33Catalogue.From(cfg);
        var fails = new List<string>();
        void Line(string id, string what, bool ok) { Console.WriteLine($"  {id}: {what} {(ok ? "GREEN" : "RED")}"); if (!ok) fails.Add(id); }
        int Idx(string id) => cat.Find(id).index;
        Console.WriteLine($"shop33 effects: {cat.Count} upgrades, prices [{string.Join(", ", cat.items.Select(i => $"{i.id} {i.price}"))}]");
        // 1. KESTREL charge -30 %: a hold of the hold + 0.7 x full seconds + 0.05 s fires a charged shot only with the upgrade
        {
            int c = Shop33One(Idx("kestrelCharge"));
            double full = cfg.Num("design.n31.charge.revolver.fullSeconds"), mult = cat.items[Idx("kestrelCharge")].value;
            SurvivalSim a = Shop33Sim(cfg, map, c, true), b = Shop33Sim(cfg, map, c, false);
            int fa = a.loadout.charge27 != null ? a.loadout.charge27.FullSteps(0) : -1, fb = b.loadout.charge27 != null ? b.loadout.charge27.FullSteps(0) : -1;
            var on = Shop33Charge(cfg, map, c, true, "revolver", full * mult, 1);
            var off = Shop33Charge(cfg, map, c, false, "revolver", full * mult, 1);
            Line("kestrelCharge", $"full charge {fa} steps on vs {fb} off (x{(fb > 0 ? fa / (double)fb : 0):0.00}); a {full * mult:0.00} s charge fires {on.charged} charged on vs {off.charged} off", fa > 0 && fb > 0 && fa < fb && on.charged == 1 && off.charged == 0);
        }
        // 2. THUNDERCLAP slug +1 pierce: a full slug through a line of three bags hits two with the upgrade, one without
        {
            int c = Shop33One(Idx("slugPierce"));
            double full = cfg.Num("design.n31.charge.shotgun.fullSeconds");
            var on = Shop33Charge(cfg, map, c, true, "shotgun", full, 3);
            var off = Shop33Charge(cfg, map, c, false, "shotgun", full, 3);
            Line("slugPierce", $"a full slug through 3 bags in a line: {on.hits} hit(s) in {on.charged} shot on vs {off.hits} in {off.charged} off", on.charged == 1 && off.charged == 1 && on.hits == off.hits + 1);
        }
        // 3. STITCHER detonate +25 %
        {
            int c = Shop33One(Idx("detonate"));
            double on = Shop33Detonate(cfg, map, c, true), off = Shop33Detonate(cfg, map, c, false), want = cat.items[Idx("detonate")].value;
            Line("detonate", $"the detonate's mean hit {on:0.00} on vs {off:0.00} off = x{(off > 0 ? on / off : 0):0.000} (want x{want})", off > 0 && Math.Abs(on / off - want) < 0.01);
        }
        // 4. PULSE parry window +40 %
        {
            int c = Shop33One(Idx("parryWindow"));
            SurvivalSim a = Shop33Sim(cfg, map, c, true), b = Shop33Sim(cfg, map, c, false);
            int wa = a.lt31 != null ? a.lt31.WindowSteps : -1, wb = b.lt31 != null ? b.lt31.WindowSteps : -1;
            Line("parryWindow", $"the LT parry window {wa} steps on vs {wb} off (x{(wb > 0 ? wa / (double)wb : 0):0.00}; want x{cat.items[Idx("parryWindow")].value})", wa > 0 && wb > 0 && wa > wb);
        }
        // 5. +1 grapple charge
        {
            int c = Shop33One(Idx("grappleCharge"));
            SurvivalSim a = Shop33Sim(cfg, map, c, true), b = Shop33Sim(cfg, map, c, false);
            Line("grappleCharge", $"grapple charges at the start {a.grappleCharges32} (max {a.grapple32Rules?.charges}) on vs {b.grappleCharges32} (max {b.grapple32Rules?.charges}) off; grapple32 {(a.Grapple32On ? "on" : "OFF")}", a.Grapple32On && a.grappleCharges32 == b.grappleCharges32 + 1);
        }
        // 6. dash recharge -20 %
        {
            int c = Shop33One(Idx("dashCooldown"));
            int on = Shop33DashRefill(cfg, map, c, true), off = Shop33DashRefill(cfg, map, c, false);
            Line("dashCooldown", $"one dash's cost back in {on} steps on vs {off} off (x{(off > 0 ? on / (double)off : 0):0.000}; want x{cat.items[Idx("dashCooldown")].value})", on > 0 && off > 0 && Math.Abs(on / (double)off - cat.items[Idx("dashCooldown")].value) < 0.03);
        }
        // 7. +20 max health
        {
            int c = Shop33One(Idx("maxHealth"));
            SurvivalSim a = Shop33Sim(cfg, map, c, true), b = Shop33Sim(cfg, map, c, false);
            Line("maxHealth", $"max {a.health.max} hp {a.health.hp} on vs max {b.health.max} hp {b.health.hp} off", a.health.max == b.health.max + cat.items[Idx("maxHealth")].value && a.health.hp == a.health.max);
        }
        // 8. a kill heals 5
        {
            int c = Shop33One(Idx("killHeal"));
            var on = Shop33KillHeal(cfg, map, c, true);
            var off = Shop33KillHeal(cfg, map, c, false);
            Line("killHeal", $"one kill from 50 hp: {on.hp} hp ({on.kills} kill) on vs {off.hp} ({off.kills} kill) off", on.kills == 1 && off.kills == 1 && on.hp == off.hp + cat.items[Idx("killHeal")].value);
        }
        // 9. nothing carries over: every slot's upgrade, then a run with the row off on the same sim
        {
            int all = Shop33One(Idx("kestrelCharge")) | Shop33One(Idx("parryWindow")) | Shop33One(Idx("grappleCharge")) | Shop33One(Idx("maxHealth"));
            SurvivalSim fresh = Shop33Sim(cfg, map, 0, false);
            SurvivalSim sim = Shop33Sim(cfg, map, all, true);
            string up = $"charge {sim.loadout.charge27?.FullSteps(0)}, window {sim.lt31?.WindowSteps}, grapple {sim.grappleCharges32}/{sim.grapple32Rules?.charges}, max {sim.health.max}";
            sim.n33Override = new N33Blocks();
            sim.StartRun(3302, null, null);
            string back = $"charge {sim.loadout.charge27?.FullSteps(0)}, window {sim.lt31?.WindowSteps}, grapple {sim.grappleCharges32}/{sim.grapple32Rules?.charges}, max {sim.health.max}";
            string want = $"charge {fresh.loadout.charge27?.FullSteps(0)}, window {fresh.lt31?.WindowSteps}, grapple {fresh.grappleCharges32}/{fresh.grapple32Rules?.charges}, max {fresh.health.max}";
            Line("carry", $"with four upgrades [{up}], the next run with the row off [{back}], a fresh sim [{want}]", back == want && up != want);
        }
        Console.WriteLine(fails.Count == 0 ? "shop33 effects GREEN" : $"shop33 effects RED: {string.Join(", ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    /// <summary>`shop33 tape [--seed n] [--seconds s]`: walk29's pilot plays n32-foundry with shop33 on and four upgrades equipped, taped (bank33 0: the wallet is not a run input);
    /// the tape goes through its bytes and replays on a fresh composition: EQUAL. The same tape with nothing bought must DIFFER, and the run's points
    /// must keep NIGHT32's start (points.start: the wallet never touches a run's points).</summary>
    static void Shop33Tape(string[] args, ref int code)
    {
        uint seed = uint.Parse(Opt30(args, "--seed", "2"), CultureInfo.InvariantCulture);
        double seconds = double.Parse(Opt30(args, "--seconds", "90"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        Shop33Catalogue cat = Shop33Catalogue.From(cfg);
        int owned = Shop33One(cat.Find("kestrelCharge").index) | Shop33One(cat.Find("slugPierce").index) | Shop33One(cat.Find("dashCooldown").index) | Shop33One(cat.Find("killHeal").index);
        const int bank = 0; // the lead, 11:04: the run's own points stay NIGHT32's; bank33 stays 0
        string path = Path.Combine("Design", "levels", "n32-foundry.json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        N33Blocks n33 = N33Blocks.ForceOff ? N33Blocks.Off : N33Blocks.From(cfg);
        n33.shop33 = true;
        sim.n33Override = n33;
        sim.shop33Owned = owned;
        sim.bank33 = bank;
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
        double startPoints = -1;
        RunTape tape = TapeScript.Record(sim, seed, MovementMath.StepsFor(seconds, sim.step), "shop33-tape", null, s => startPoints = s.economy.total, scripted: false);
        ap.Detach();
        byte[] bytes = tape.ToBytes();
        RunTape back = RunTape.FromBytes(bytes);
        bool header = back.shop33 == owned && back.bank33 == bank && N33Blocks.FromBits(back.n33).shop33;
        Console.WriteLine($"shop33 tape: {sim.run.steps} steps on n32-foundry, {tape.opsLength} op bytes ({bytes.Length} bytes); header shop33 {back.shop33} (want {owned}), bank33 {back.bank33}, n33 bits {back.n33}: {(header ? "PASS" : "FAIL")}; points at the start {startPoints} (want points.start {cfg.Num("points.start")}), at the end {sim.economy.total}, earned {sim.economy.earned}, kills {sim.run.stats.kills}, healed by kills {sim.shop33Healed}, effects [{sim.shop33Effects}]");
        TapeResult rep = TapeReplay.Replay(cfg, map, back);
        bool light = rep.Sim != null && TapeReplay.Light(rep.Sim) == TapeReplay.Light(sim);
        Console.WriteLine($"shop33 tape replay: {(rep.Equal ? "EQUAL" : "DIFFERS: " + rep.Line)} ({rep.Checkpoints} of {rep.CheckpointsTotal} checkpoints), the last step's hash {(light ? "EQUAL" : "DIFFERS")}");
        RunTape none = RunTape.FromBytes(bytes);
        none.shop33 = 0;
        none.bank33 = 0;
        TapeResult repNone = TapeReplay.Replay(cfg, map, none);
        Console.WriteLine($"shop33 tape with nothing bought: {(repNone.Equal ? "EQUAL (WRONG: the upgrades are not what is played)" : "DIFFERS (right)")}");
        bool ok = header && rep.Equal && light && !repNone.Equal && startPoints == cfg.Num("points.start");
        Console.WriteLine($"shop33 tape: {(ok ? "PASS" : "FAIL")}");
        code = ok ? 0 : 1;
    }

    /// <summary>One walk29 run of a level (the n32 plan): the points the run earned and whether every playable room cleared.</summary>
    static (double earned, double total, bool done, double seconds) Shop33WalkOnce(GameConfig cfg, string level, uint seed, double cap, Action<SurvivalSim> beforeStart = null)
    {
        string path = Path.Combine("Design", "levels", $"n32-{level}.json");
        var json0 = PortData.Obj(PortData.LoadJson(Path.GetFullPath(path)));
        LevelDef lv0 = LevelDef.From(json0);
        int playable = lv0.rooms.FindAll(r => r.kind != "boss").Count;
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
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
        beforeStart?.Invoke(sim);
        sim.StartRun(seed, null, null);
        int max = MovementMath.StepsFor(cap, sim.step);
        int st = 0;
        for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            Campaign24 c = sim.campaign;
            if (c != null && c.clearSteps.Count >= playable) break;
        }
        bool done = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
        double earned = sim.economy.earned, total = sim.economy.total;
        ap.Detach();
        return (earned, total, done, st * sim.step);
    }

    static double Shop33P50(List<double> xs)
    {
        if (xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(x => x).ToList();
        return s[s.Count / 2];
    }

    static void Shop33Earn(string[] args, ref int code)
    {
        int seeds = int.Parse(Opt30(args, "--seeds", "5"), CultureInfo.InvariantCulture);
        int first = int.Parse(Opt30(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(Opt30(args, "--cap", "900"), CultureInfo.InvariantCulture);
        string[] levels = Opt30(args, "--levels", "foundry,iceline,kiln,span,spire").Split(',');
        GameConfig cfg = Config();
        Console.WriteLine($"shop33 earn: walk29's pilot on Design/levels/n32-<level>.json, playable rooms (boss excluded), the design config as it is, {seeds} seeds from {first}");
        var p50s = new List<double>();
        foreach (string level in levels)
        {
            var earns = new List<double>();
            var all = new List<string>();
            int done = 0;
            for (int s = 0; s < seeds; s++)
            {
                var r = Shop33WalkOnce(cfg, level, (uint)(first + s), cap);
                all.Add($"{r.earned:0}{(r.done ? "" : "*")}");
                earns.Add(r.earned);
                if (r.done) done++;
            }
            double p50 = Shop33P50(earns);
            p50s.Add(p50);
            Console.WriteLine($"shop33 earn n32-{level}: p50 {p50:0} (min {earns.Min():0}, max {earns.Max():0}), cleared {done}/{seeds}; runs [{string.Join(", ", all)}] (* = not every room cleared)");
        }
        Console.WriteLine($"shop33 earn SUMMARY: level p50s [{string.Join(", ", p50s.Select(x => x.ToString("0", CultureInfo.InvariantCulture)))}], typical earn (p50 of the level p50s) {Shop33P50(p50s):0}");
    }
}
