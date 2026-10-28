// NIGHT32 builder B (design.n32.bosses32; DESIGN32 B): `boss32 <lantern|stag|wyrm|loom|all> [--seeds N] [--cap S]` and `camp32 <boss|all> [--seed n] [--cap S]`.
// Declared and called by the lead's Program.cs (N32Boss). The row is read from Design/unity-config.json (design.n32.bosses32.enabled); --off32 forces it off (NIGHT31's game:
// the BEFORE table).
// boss32: per boss, the moving pilot over seeds 1..N (the NIGHT31 pilots, mortal, respawning: THE LANTERN's rider LanternRun30 with lantern31 + charge; THE STAG's StagPilot31;
// THE KILN WYRM's WyrmPilot31 punch-and-slug; THE LOOM's lag bot on the standalone rig, as camp31 used): the kill time p10 / p50 / p90 (an unkilled run counts as the cap), deaths,
// the share of hits that landed full (or the bonus), the hits under 25 % of their damage, the mean landed share, and the seconds per minute of fight in which no hit could land.
// PASS (DESIGN32 B's band): every seed killed, p50 in 45-75 s, p90 at most 100 s.
// camp32: camp31's campers (Harness.N31.Boss.cs: centre, edge, highest, farthest, the Stag's wall corner). A camper LOSES when it dies or loses half its health within 30 s having
// taken under half the bar (camp31's "under a third of the pilot's rate" no longer fits: every hit now lands at least 25 %); plus the B rule: a camper whose shots landed must have
// dealt at least 25 % of their damage on average (no spot where the boss can't be hurt).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N32Boss(string cmd, string[] args, ref int code)
    {
        if (cmd == "hurt32") { code = Hurt32(); return; }
        if (cmd != "boss32" && cmd != "camp32") return;
        code = 0;
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = 20, seed = 1;
        double cap = 240;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") int.TryParse(args[i + 1], out seeds);
            if (args[i] == "--seed") int.TryParse(args[i + 1], out seed);
            if (args[i] == "--cap") double.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out cap);
        }
        sets30.Clear();
        pistolFist32 = Array.IndexOf(args, "--pistolfist") >= 0;
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        try
        {
            foreach (string b in new[] { "lantern", "stag", "wyrm", "loom", "loommodel" })
                if (which == b || which == "all" && b != "loommodel" && !(cmd == "camp32" && b == "loommodel")) code |= cmd == "boss32" ? Boss32Sweep(b, seeds, cap) : Camp32(b, (uint)seed, cap);
        }
        finally
        {
            SurvivalSim.Wyrm24Dir = null;
            SurvivalSim.Boss32Probe = null;
        }
    }

    /// <summary>`hurt32`: each boss's hit volume (the sim, after 3 steps of its fight with the rows as the config has them) beside its drawn model (the views' formulas, read from
    /// LanternView.cs:165-175, Boss31View.cs:373-383, WyrmView.cs:139, Boss30View.cs:293 and UnityTuning: plate 0.12 m, leg share 0.45, joint 0.8 or 0.75 with the lead's patch).
    /// PASS per boss: the hit volume's smallest axis at least 1.3x the drawn model's on that axis.</summary>
    static int Hurt32()
    {
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        int bad = 0;
        void Line(string boss, string hit, string drawn, double ratio) { bool ok = ratio >= 1.3 - 1e-9; if (!ok) bad++; Console.WriteLine($"  {(ok ? "PASS" : "RED ")} {boss,-8} hit {hit,-34} drawn {drawn,-34} ratio {ratio:0.00}"); }
        try
        {
            CampRig31 r = CampRig31For("lantern31", 1);
            for (int i = 0; i < 3; i++) r.sim.Step();
            WyrmBox b = r.sim.lantern27.boxes[0];
            double drawn = 2 * b.hr + 2 * 0.12; // LanternView: the body 2 x hr, a 0.12 m plate on each face
            Line("lantern", $"cube {2 * b.hr:0.00} m (Lantern27, not B's)", $"body {2 * b.hr:0.00} + plates = {drawn:0.00} m", 2 * b.hr / drawn);
            r = CampRig31For("stag", 1);
            for (int i = 0; i < 3; i++) r.sim.Step();
            Stag31 st = r.sim.stag31;
            double hs = st.on32 ? st.hitScale32 : 1;
            Line("stag", $"box {2 * st.box.hr * hs:0.0} x {2 * st.box.hu * hs:0.0} x {2 * st.box.hf * hs:0.0} m", $"torso {2 * st.box.hr:0.0} x {2 * st.box.hu * 0.55:0.0} x {2 * st.box.hf:0.0} m", hs);
            r = CampRig31For("wyrm31", 1);
            for (int i = 0; i < 3; i++) r.sim.Step();
            WyrmBox h = r.sim.wyrm24.boxes[0];
            Line("wyrm", $"head {2 * h.hr:0.0} x {2 * h.hu:0.0} x {2 * h.hf:0.0} m (Wyrm, not B's)", "the same boxes (WyrmView)", 1.0);
            r = CampRig31For("loom31", 1);
            for (int i = 0; i < 3; i++) r.sim.Step();
            double jr = r.sim.loom30.p.jointRadius;
            string tuning = File.ReadAllText(Path.Combine(Root, "..", "Assets", "Scripts", "Game", "UnityTuning.Boss30.cs"));
            double js = tuning.Contains("Loom30JointScale = 0.75f") ? 0.75 : 0.8;
            Line("loom", $"joint sphere d {2 * jr:0.00} m", $"joint cube {2 * jr * js:0.00} m (x{js})", 1 / js);
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
        Console.WriteLine($"hurt32 {(bad == 0 ? "PASS" : "RED")}: {4 - bad} of 4 bosses hit at least 1.3x their drawing");
        return bad == 0 ? 0 : 1;
    }

    sealed class Run32
    {
        public bool killed;
        public double seconds;
        public int deaths, hits, full, fightSteps, zeroSteps, low, paired;
        public double raw, landed;
        public string note = "";
    }

    static bool Bosses32On(GameConfig cfg) => !N32Blocks.ForceOff && cfg.Design("n32.bosses32.enabled");

    static void Take32(Run32 r, SurvivalSim sim)
    {
        if (sim == null) return;
        Boss32Stats s = sim.boss32Stats;
        r.hits = s.hits; r.full = s.fullHits; r.fightSteps = s.fightSteps; r.zeroSteps = s.zeroSteps; r.low = s.lowHits; r.paired = s.paired; r.raw = s.pairedRaw; r.landed = s.pairedLanded;
    }

    /// <summary>One pilot run against `boss` (seed), at most `cap` seconds of fight.</summary>
    static Run32 Boss32Run(string boss, uint seed, double cap)
    {
        double dt = 1.0 / 120;
        var r = new Run32();
        SurvivalSim last = null;
        SurvivalSim.Boss32Probe = s => last = s;
        if (boss == "lantern")
        {
            LanternRow30 lr = LanternRun30(Cfg30("lantern30"), seed, (int)(cap * 120), 0, true, false, new N31Blocks { lantern31 = true, charge = true });
            r.killed = lr.killed; r.seconds = lr.steps * dt; r.deaths = lr.deaths;
            r.note = $"rides {lr.rides}, breaks {last?.lantern31?.breaks}, carried over the clamps {last?.boss32LanternCarried:0}";
            Take32(r, last);
        }
        else if (boss == "stag")
        {
            StagRow31 sr = StagPilotRun31(seed, cap, false);
            r.killed = sr.killed; r.seconds = sr.seconds; r.deaths = sr.deaths;
            r.note = $"punches {sr.punches}, back {last?.stag31?.backPunches}, crystals {last?.stag31?.crystalBreaks}";
            Take32(r, last);
        }
        else if (boss == "wyrm")
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg30("wyrm30"), Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.n26Override = new N26Blocks { level3 = true };
            sim.n29Override = new N29Blocks { boss1 = true };
            sim.n30Override = new N30Blocks { wyrm30 = true };
            sim.n31Override = new N31Blocks { wyrm31 = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed, null, null);
            sim.husks.frozen = true;
            if (!sim.Wyrm24Begin()) { r.note = "no fight"; return r; }
            sim.Respawn(sim.Wyrm24Spawn, 0);
            Wyrm w = sim.wyrm24;
            foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
            PistolFist32(sim); // --pistolfist: the THUNDERCLAP taken away (WyrmPilot31 then keeps the KESTREL / PULSE it holds)
            var si = (ScriptedInput)sim.input;
            var wp = new WyrmPilot31();
            int step = 0, capSteps = (int)(cap * 120);
            while (w.Alive && step < capSteps)
            {
                wp.Step(sim, step);
                si.SetLook(wp.Move.yaw, wp.Move.pitch);
                si.Set(wp.Move, wp.Combat);
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
                if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
                {
                    r.deaths++;
                    sim.downSteps = 0;
                    sim.flow = Skygrind.Game.Flow.Playing;
                    sim.health.Reset();
                    sim.Wyrm24Begin();
                    sim.Respawn(sim.Wyrm24Spawn, 0);
                }
            }
            r.killed = !w.Alive; r.seconds = step * dt;
            r.note = $"staggers {sim.wyrm30Staggers}, slug hits {sim.wyrm31?.slugHits32}, pool {w.body.maxHp}, dealt {sim.boss32Stats.dealt:0}, phase {w.phase}";
            Take32(r, sim);
        }
        else if (boss == "loom") Loom32Sim(seed, cap, r);
        else if (boss == "loommodel") Loom32Model(seed, cap, r);
        SurvivalSim.Boss32Probe = null;
        return r;
    }

    /// <summary>`--pistolfist`: every pilot keeps only the KESTREL and the PULSE gauntlet (DESIGN32 B: no hard lock on one weapon; band 150 s).</summary>
    static bool pistolFist32;

    static void PistolFist32(SurvivalSim sim)
    {
        if (!pistolFist32) return;
        foreach (string w in new[] { "shotgun", "nailgun" }) sim.loadout.owned.Remove(w);
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
    }

    /// <summary>THE LOOM on the FULL sim (the lead's 19:07 item 6): camp31's loom31 rig (a CAMPAIGN run on n30-span, DOORFREE, the s8 fight begun), LoomPilot32 at the controls
    /// (the Player reel's pilot), mortal: a death puts the pilot back at the room's entry with full health (the Loom keeps its bar: the campaign's own retry is not modelled).</summary>
    static void Loom32Sim(uint seed, double cap, Run32 r)
    {
        SurvivalSim last = null;
        SurvivalSim.Boss32Probe = s => last = s;
        CampRig31 rig = CampRig31For("loom31", seed);
        if (rig == null) { r.note = "no fight"; return; }
        SurvivalSim sim = rig.sim;
        Vec3 entry = rig.spots[1].at; // a deck corner (the room's spawn sits off the deck: a respawn there walked through the lava; the middle is the pit after the break)
        sim.Respawn(entry, 0);
        PistolFist32(sim);
        Loom30 l = sim.loom30;
        var si = (ScriptedInput)sim.input;
        var pilot = new LoomPilot32 { pistolFist = pistolFist32 };
        sim.mover.invulnerable = false;
        var hurtBy = new Dictionary<string, double>();
        sim.events.On<HurtEvent>("hurt", e => { if (e.owner != "player") return; string k = e.source ?? "?"; hurtBy[k] = (hurtBy.TryGetValue(k, out double v) ? v : 0) + e.amount; });
        int step = 0, capSteps = (int)(cap * 120), hurt0 = -1;
        while (l.Alive && l.phase != "dying" && (hurt0 < 0 || step - hurt0 < capSteps) && step < capSteps + 60 * 120)
        {
            if (hurt0 < 0 && l.Hurtable) hurt0 = step;
            pilot.Step(sim, step);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Respawn(entry, 0);
            }
        }
        r.killed = !l.Alive || l.phase == "dying";
        r.seconds = (step - Math.Max(0, hurt0)) / 120.0;
        r.note = $"pool {l.body.maxHp}, hp left {l.body.hp:0}, phase {l.phase}, dealt by the player {sim.boss32Stats.dealt:0}, corners {pilot.switches}, hops {pilot.hops}, full {l.fullHits} / light {l.lightHits}, drops {sim.loom31?.drops} ({sim.loom31?.dropHits} hit), break {sim.loom31?.breakState}, sweeps {l.sweeps}, lava hits {sim.loom31?.lavaHits}, hurt by {string.Join(" ", new List<string>(hurtBy.Keys).ConvertAll(k => k + " " + hurtBy[k].ToString("0")))}";
        Take32(r, last ?? sim);
    }

    /// <summary>THE LOOM's lag bot on the standalone rig (Harness.N30.Boss.cs LoomBotRun, the camp31 pilot) with NIGHT31's rules (Loom31) and, while the row is on, the row's numbers
    /// (Loom31.Set32) and pool (hpMult). No full sim: deaths are the cuts; the no-damage time is the steps of p1 the Loom was not hurtable (none on this rig).</summary>
    static void Loom32Model(uint seed, double cap, Run32 r)
    {
        GameConfig cfg = Cfg30("span");
        var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine(Root, "..", "Design", "levels", "n30", "span.plan.json"))));
        Vec3[] legs = null;
        foreach (object ro in PortData.Arr(plan["rooms"]))
        {
            var rm = PortData.Obj(ro);
            if (rm.ContainsKey("loomLegs")) { var a = PortData.Arr(rm["loomLegs"]); legs = new Vec3[4]; for (int i = 0; i < 4; i++) legs[i] = PortData.V3(a[i]); }
        }
        Vec3 deck = legs != null ? new Vec3((legs[0].x + legs[2].x) * 0.5, legs[0].y, (legs[0].z + legs[2].z) * 0.5) : new Vec3(0, 0, -218);
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
        bool cutNow = false;
        loom.playerHanging = () => mover.rope.phase == RopePhase.Attached;
        loom.ropeAnchor = () => mover.rope.anchor;
        loom.cutLine = drop => { mover.rope.phase = RopePhase.None; cutNow = true; };
        loom.Place(deck, legs);
        kit.bosses["loom"] = loom;
        kit.Start("loom");
        bool on = Bosses32On(cfg) && cfg.Design("n32.bosses32.loom.enabled");
        var l31 = new Loom31(cfg, Loom31Params.From(cfg), events, target);
        l31.Reset();
        l31.Hook(loom);
        if (on)
        {
            l31.Set32(cfg, true, loom);
            double m = Boss32Cfg.Num(cfg, "loom", "hpMult", 1);
            loom.body.maxHp = JsMath.Round(loom.body.maxHp * m);
            loom.body.hp = loom.body.maxHp;
        }
        var hand = new Mulberry32(seed * 7919u + 13u);
        int state = 0, timer = MovementMath.StepsFor(swingSeconds, dt), leg = (int)(seed % 4), react = -1, playSteps = 0;
        double fireAcc = 0, heatLeft = heat, lockLeft = 0;
        int step = 0, capSteps = (int)(cap * 120) + 6 * 120;
        while (loom.Alive && loom.phase != "dying" && step < capSteps && playSteps < cap * 120)
        {
            loom.Step(dt); kit.Step(dt); step++;
            if (loom.phase != "p1") continue;
            l31.Step(loom, mover.rope.phase == RopePhase.Attached, state == 2, dt);
            playSteps++;
            r.fightSteps++;
            if (!loom.Hurtable) r.zeroSteps++;
            if (state == 0 && --timer <= 0)
            {
                state = 1;
                Vec3 k = loom.joints[2 * leg];
                mover.Reset(k.x, k.y - 3.5, k.z);
                mover.rope.phase = RopePhase.Attached;
                mover.rope.anchor = k;
                react = -1;
            }
            else if (state == 1)
            {
                if ((loom.CutTelling || l31.ShakeTelling) && react < 0) react = hand.Next() < missChance ? int.MaxValue : MovementMath.StepsFor(reactSeconds, dt);
                if (react >= 0 && react != int.MaxValue && --react == 0) { mover.rope.phase = RopePhase.None; state = 2; timer = MovementMath.StepsFor(deckSeconds, dt); leg = (leg + 1) % 4; mover.Reset(deck.x, deck.y + 0.05, deck.z); }
            }
            else if (state == 2 && --timer <= 0) { state = 0; timer = MovementMath.StepsFor(swingSeconds, dt); }
            if (cutNow) { cutNow = false; r.deaths++; state = 2; timer = MovementMath.StepsFor(recoverSeconds, dt); leg = (leg + 1) % 4; mover.Reset(deck.x, deck.y + 0.05, deck.z); }
            if (lockLeft > 0) { lockLeft -= dt; if (lockLeft <= 0) heatLeft = heat; continue; }
            if (state == 0) continue;
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
                r.hits++; r.paired++; r.raw += dmg; r.landed += landed;
                if (landed / dmg < 0.245) r.low++;
                if (loom.fullHits > full0) r.full++;
            }
        }
        r.killed = !loom.Alive || loom.phase == "dying";
        r.seconds = playSteps * dt;
        r.note = $"shake tells {l31.shakeTells}, drops {l31.drops}, pool {loom.body.maxHp}";
    }

    static double Pct32(List<double> v, double q)
    {
        if (v.Count == 0) return 0;
        var c = new List<double>(v);
        c.Sort();
        return c[(int)Math.Round(q * (c.Count - 1))];
    }

    static readonly Dictionary<string, double> pilot32 = new Dictionary<string, double>();

    static int Boss32Sweep(string boss, int seeds, double cap)
    {
        var times = new List<double>();
        int killed = 0, deaths = 0, hits = 0, full = 0, fight = 0, zero = 0, low = 0, paired = 0;
        double raw = 0, landed = 0;
        string notes = "";
        GameConfig c0 = Cfg30();
        bool on = Bosses32On(c0) && c0.Design("n32.bosses32." + boss + ".enabled");
        for (int s = 1; s <= seeds; s++)
        {
            Run32 r = Boss32Run(boss, (uint)s, cap);
            times.Add(r.killed ? r.seconds : cap);
            if (r.killed) killed++;
            deaths += r.deaths; hits += r.hits; full += r.full; fight += r.fightSteps; zero += r.zeroSteps; low += r.low; paired += r.paired; raw += r.raw; landed += r.landed;
            if (s <= 3) notes += $" [s{s} {(r.killed ? r.seconds.ToString("0.0") + " s" : "NOT " + r.seconds.ToString("0") + " s")}; {r.note}]";
        }
        double p10 = Pct32(times, 0.1), p50 = Pct32(times, 0.5), p90 = Pct32(times, 0.9);
        pilot32[boss] = p50;
        // the band (the lead 20:10, GG "bosses still too tanky!"): the pilot kills in 25-40 s, p90 <= 45 s; --pistolfist: <= 90 s (was 45-75 / 100 / 150)
        bool pass = pistolFist32 ? killed == seeds && p90 <= 90 : killed == seeds && p50 >= 25 && p50 <= 40 && p90 <= 45;
        Console.WriteLine($"boss32 {boss} (bosses32 {(on ? "ON" : "OFF")}): seeds {seeds}, killed {killed}/{seeds}, kill s p10 {p10:0.0} / p50 {p50:0.0} / p90 {p90:0.0}, deaths/run {(double)deaths / seeds:0.0}, " +
            $"full-share hits {(hits > 0 ? 100.0 * full / hits : 0):0} % of {hits}, hits under 25 % {(paired > 0 ? 100.0 * low / paired : 0):0} %, mean share {(raw > 0 ? 100 * landed / raw : 0):0} %, no-damage {(fight > 0 ? 60.0 * zero / fight : 0):0.0} s/min");
        Console.WriteLine($"  notes:{notes}");
        Console.WriteLine($"boss32 {boss} {(pass ? "PASS" : "RED")} {(pistolFist32 ? "(pistol+fist band: every seed killed, p90 <= 90 s)" : "(band: every seed killed, p50 25-40 s, p90 <= 45 s)")}");
        return pass ? 0 : 1;
    }

    static int Camp32(string boss, uint seed, double cap)
    {
        string rigName = boss == "lantern" ? "lantern31" : boss == "wyrm" ? "wyrm31" : boss == "loom" ? "loom31" : "stag";
        CampRig31 probe = CampRig31For(rigName, seed);
        if (probe == null) { Console.WriteLine($"camp32 {boss}: NOT BUILT"); return 1; }
        Run32 pr = Boss32Run(boss, seed, cap);
        double pilotRate = pr.killed && pr.seconds > 0 ? 1.0 / pr.seconds : 0.5 / cap;
        bool on = Bosses32On(probe.sim.cfg) && probe.sim.cfg.Design("n32.bosses32." + boss + ".enabled");
        Console.WriteLine($"camp32 {boss} (bosses32 {(on ? "ON" : "OFF")}; level {probe.level}; {probe.note}): pilot seed {seed} {(pr.killed ? "KILLED in " + pr.seconds.ToString("0.0") + " s" : "NOT killed in " + pr.seconds.ToString("0") + " s")} = {pilotRate * 100:0.00} % of the bar / s; a third = {pilotRate / 3 * 100:0.00} %/s");
        Console.WriteLine($"  {"camper",-52} {"alive",8} {"hp lost",8} {"dmg %/s",8} {"hits",5} {"share",6} {"weapons",-20} verdict");
        int bad = 0;
        for (int i = 0; i < probe.spots.Count; i++)
        {
            SurvivalSim last = null;
            SurvivalSim.Boss32Probe = s => last = s;
            CampRow31 row = CampRun31(rigName, seed, i, false);
            SurvivalSim.Boss32Probe = null;
            if (row == null) { Console.WriteLine($"  spot {i}: rig failed"); bad++; continue; }
            Boss32Stats st = last?.boss32Stats;
            int h = st != null ? st.paired : 0;
            double share = st != null ? st.Share : 0;
            // camp32's rule: the B rule makes every hit land at least 25 %, so a camper's damage rate is no longer under a third of the pilot's (KESTREL at 40 % is about half the
            // rider's): a camper LOSES when it dies or loses half its health within 30 s having taken under half the bar (it cannot win before it falls); camp31's rate test is
            // kept as information ("slow")
            bool hurt = row.died || row.lost >= 0.5, slow = row.rate < pilotRate / 3, smallTake = row.rate * row.alive < 0.5;
            bool loses = hurt && (slow || smallTake);
            bool shareOk = h == 0 || share >= 0.245;
            if (!loses || !shareOk) bad++;
            string why = !loses ? (hurt ? "CAMPS (took half the bar before falling)" : "CAMPS (safe: not hurt enough)") : $"LOSES (took {row.rate * row.alive * 100:0} % of the bar{(slow ? ", slow" : "")})";
            if (!shareOk) why += $"; LOW SHARE (under 25 %)";
            Console.WriteLine($"  {row.name,-52} {(row.died ? "DIED " : "") + row.alive.ToString("0.0") + " s",8} {row.lost * 100,7:0} % {row.rate * 100,8:0.00} {h,5} {share * 100,5:0} % {row.weapons,-20} {why}");
        }
        Console.WriteLine($"camp32 {boss} {(bad == 0 ? "PASS" : "RED")}: {probe.spots.Count - bad} of {probe.spots.Count} spots pass (every camper dies or loses half its health in 30 s having taken under half the bar, AND its landed hits average at least 25 % of their damage)");
        return bad == 0 ? 0 : 1;
    }
}
