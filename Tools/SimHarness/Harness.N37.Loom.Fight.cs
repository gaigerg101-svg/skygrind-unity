// NIGHT37 builder O: THE LOOM's fight checks (the second half of Harness.N37.Loom.cs).
//   loom37 fight [--seeds 1] [--trace]   one fight from the canyon's start ledge: every tell's kind and wind-up, tears (pull / LT), windows, legs, the kill.
//   loom37 band [--seeds N] [--cap S]    Loom37Pilot (mortal; falls and deaths restart the leg) over seeds 1..N: kill time from the begin p10 / p50 / p90,
//                                        falls, tears, parries; the share of damage dealt while grounded and still (want ~0).
//                                        PASS: every seed killed, p50 15-22 s, p90 <= 26 s, still share < 5 %.
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Loom37Extra(string what, int seeds, double cap, bool off, ref int fails)
    {
        if (off) return;
        if (what == "room" || what == "all") fails += Loom37RoomCheck();
        if (what == "camp" || what == "all") fails += Loom37CampCheck();
        if (what == "fight" || what == "all") fails += Loom37Fight(1, cap, true).ok ? 0 : 1;
        if (what == "band" || what == "all") fails += Loom37Band(seeds, cap);
    }

    /// <summary>loom37 room: a campaign run with DOORFREE (the reel's way) put on the canyon's start ledge: does the campaign enter o3 (its room
    /// title, its begin)?</summary>
    static int Loom37RoomCheck()
    {
        SurvivalSim sim = Loom37Sim(1, true);
        for (int i = 0; i < 10; i++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        sim.Respawn(new Vec3(start.at.x, 0.05, start.at.z), 0);
        string entered = null;
        sim.events.On<RoomEvent>("roomEnter", e => entered = e.id);
        for (int i = 0; i < 30; i++) sim.Step();
        bool ok = sim.campaign.Room?.id == "o3";
        Console.WriteLine($"room: zone at the start ledge '{sim.zones.ZoneAt(new Vec3(start.at.x, 1.05, start.at.z))}', campaign room {sim.campaign.Room?.id} ({sim.campaign.Room?.name}), roomEnter {entered ?? "none"}, loom begun {sim.loom37?.Alive}: {Pass37O(ok)}");
        return ok ? 0 : 1;
    }

    /// <summary>loom37 camp: a player who camps on purpose. Leg 1 and leg 2 (the web wall is legs 2-3): the player stands still on a tower top
    /// (t1w, then t2w) for 8 s. PASS when the hide rule severs that tower's thread (leg 1 after hideS, leg 2 after hideS x hideLeg2Scale) and, in
    /// leg 2, the camp habit picks the WEB WALL (its RED tell, then the sheet solid between tower pair 2 or 3).</summary>
    static int Loom37CampCheck()
    {
        int fails = 0;
        for (int leg = 1; leg <= 2; leg++)
        {
            SurvivalSim sim = Loom37Sim(1, true);
            var start = sim.loom37Ledges.Find(l => l.id == "start");
            sim.Respawn(new Vec3(start.at.x, 0.05, start.at.z), 0);
            for (int i = 0; i < 10; i++) sim.Step();
            sim.BossRoomBegin("loom37", out _, out _);
            Loom37 L = sim.loom37;
            double dt = sim.step;
            int st = 0;
            while (L.brain.state == Boss37State.Reveal && st < 600) { sim.Step(); st++; }
            if (leg == 2)
            {
                L.brain.NextLeg();
                while (L.brain.state == Boss37State.LegChange && st < 1200) { sim.Step(); st++; }
            }
            var tower = sim.loom37Ledges.Find(l => l.id == "t" + leg + "w");
            sim.Respawn(new Vec3(tower.at.x, tower.y + 0.05, tower.at.z), 0);
            sim.mover.invulnerable = true;
            double sever = -1, webTell = -1, webSolid = -1, t0 = st * dt;
            string severed = null;
            sim.events.On<string>("loom37Sever", id => { if (sever < 0) { sever = st * dt - t0; severed = id; } });
            sim.events.On<Boss37Event>(Boss37Brain.EvTell, e => { if (e.attack == Loom37.Web && webTell < 0) webTell = st * dt - t0; });
            for (int i = 0; i < 8 * 120; i++)
            {
                sim.health.hp = sim.health.max;
                sim.Step();
                st++;
                if (L.WebSolid && webSolid < 0) webSolid = st * dt - t0;
            }
            double want = sim.cfg.Num("design.n37.loom37.hideS") * (leg >= 2 ? sim.cfg.Num("design.n37.loom37.hideLeg2Scale") : 1);
            bool sevOk = sever >= 0 && Math.Abs(sever - want) < 0.2 && severed == tower.id;
            bool webOk = leg < 2 || (webTell >= 0 && webSolid >= 0);
            Console.WriteLine($"camp leg {leg}: standing on {tower.id} 8 s: sever {(sever < 0 ? "never" : $"{sever:0.00} s ({severed})")} (want {want:0.00} s), web tell {(webTell < 0 ? "never" : $"{webTell:0.00} s")}, web solid {(webSolid < 0 ? "never" : $"{webSolid:0.00} s")}, camp picks {L.brain.campPicks}{(leg < 2 ? " (the web is legs 2-3)" : "")}: {Pass37O(sevOk && webOk)}");
            if (!(sevOk && webOk)) fails++;
        }
        return fails;
    }

    sealed class Loom37Run
    {
        public bool ok, killed;
        public double seconds, still, dealt, hpLeft;
        public int falls, deaths, tearsPull, tearsLt, parries, windows, lashHits, spoolHits, cuts, severs, pulls, yellow, red;
        public string tells = "", legs = "";
    }

    /// <summary>One fight: the player on the canyon's start ledge, BossRoomBegin("loom37"), Loom37Pilot at the controls until the kill or the cap.</summary>
    static Loom37Run Loom37Fight(uint seed, double cap, bool print)
    {
        var r = new Loom37Run();
        SurvivalSim sim = Loom37Sim(seed, true);

        var start = sim.loom37Ledges.Find(l => l.id == "start");
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!sim.loadout.owned.Contains("nailgun")) sim.loadout.AddWeapon("nailgun"); // the level's NAILGUN (a pickup in room s1)
        bool began = sim.BossRoomBegin("loom37", out Vec3 sp, out double yaw);
        Loom37 L = sim.loom37;
        if (!began || L == null || !L.Alive)
        {
            Console.WriteLine($"fight: RED: BossRoomBegin(loom37) {began}, loom {(L == null ? "null" : L.Alive.ToString())}");
            return r;
        }
        bool owned = ReferenceEquals(sim.BossRoomSystem("loom37"), L);
        if (print) Console.WriteLine($"  loadout owned [{string.Join(",", sim.loadout.owned)}], in hand {sim.loadout.Weapon}");
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot();
        sim.mover.invulnerable = false;
        int st = 0, capSteps = (int)(cap * 120);
        double dt = sim.step;
        var tellLog = new List<string>();
        var legLog = new List<string>();
        sim.events.On<Boss37Event>(Boss37Brain.EvTell, e => { tellLog.Add($"{e.attack}({e.kind},{e.seconds:0.0}s)"); if (e.kind == Tell36Kind.Yellow) r.yellow++; else r.red++; });
        sim.events.On<Boss37Event>(Boss37Brain.EvLeg, e => legLog.Add($"{st * dt:0.0}s {e.stage} leg {e.leg}"));
        sim.events.On<Loom37Event>("loom37Tear", e => { if (e.stage == "pull") r.tearsPull++; else r.tearsLt++; if (print) Console.WriteLine($"    {st * dt:0.0}s tear knee {e.knee} by {e.stage} (leg {e.leg})"); });
        int killStep = -1;
        sim.events.On<Loom37Event>("loom37Down", e => { if (killStep < 0) killStep = st; });
        sim.events.On<Loom37Event>("loom37Open", e => { r.windows++; if (print) Console.WriteLine($"    {st * dt:0.0}s CYAN window open (leg {e.leg})"); });
        sim.events.On<BossEvent>("bossHit", e =>
        {
            if (e.boss != "loom37") return;
            r.dealt += e.damage;
            if (sim.mover.grounded && JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z) < 1) r.still += e.damage;
        });
        int deaths = 0;
        while (st < capSteps && L.Alive)
        {
            if (sim.health.hp <= 0 || sim.flow != Skygrind.Game.Flow.Playing)
            {
                // a death: the leg restarts on the last ledge, full health (Campaign24's retry is not modelled here)
                deaths++;
                sim.health.hp = sim.health.max;
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
                if (sim.flow != Skygrind.Game.Flow.Playing) break;
            }
            if (!Frames37O || st % 2 == 0)
            {
                InputFrame mv = default;
                CombatInput cb = new CombatInput { weapon = -1 };
                for (int k = 0; k < (Frames37O ? 2 : 1); k++)
                {
                    // --frames: the Player reel's way (FeelReel.N37.Loom.cs): one input a frame of two sim steps, the pilot stepped twice, presses kept
                    pilot.Step(sim, st + k);
                    InputFrame m = pilot.Move;
                    m.jump.pressed |= mv.jump.pressed; m.dash.pressed |= mv.dash.pressed; m.grapple.pressed |= mv.grapple.pressed; m.sprint.pressed |= mv.sprint.pressed;
                    m.grapple.held |= mv.grapple.pressed;
                    CombatInput c = pilot.Combat;
                    c.fire.pressed |= cb.fire.pressed; c.special.pressed |= cb.special.pressed; c.punch.pressed |= cb.punch.pressed;
                    c.special.held |= cb.special.pressed; c.punch.held |= cb.punch.pressed;
                    if (c.weapon < 0) c.weapon = cb.weapon;
                    mv = m;
                    cb = c;
                }
                si.SetLook(mv.yaw, mv.pitch);
                si.Set(mv, cb);
            }
            sim.Step();
            sim.hitstopLeft = 0;
            st++;
            if (Trace37O && (st % 60 == 0 || (st < 400 && st % 10 == 0)))
                Console.WriteLine($"    {st * dt:0.0}s leg {L.brain.leg} {L.brain.state} {L.brain.Current?.id} stag {L.brain.stagger:0.0} hp {L.body.hp:0} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) ledge {sim.loom37LastLedge?.id} rope {sim.mover.rope.phase} anc ({sim.mover.rope.anchor.x:0.0},{sim.mover.rope.anchor.y:0.0},{sim.mover.rope.anchor.z:0.0}) st {sim.mover.state} ch {sim.grappleCharges32} nails [{string.Join(",", L.knees.Select(k => k.torn ? "X" : k.nails.ToString()))}] php {sim.health.hp:0} w {sim.loadout.Weapon} full {L.fullHits} chip {L.chipHits} nailsIn {L.nailsIn} chipScale {L.brain.p.chipScale}");
        }
        r.killed = !L.Alive;
        r.seconds = (killStep >= 0 ? killStep : st) * dt; // the kill: the moment it falls (the fall itself plays dieS after)
        r.falls = sim.loom37Falls;
        r.deaths = deaths;
        r.parries = L.spoolParries;
        r.lashHits = L.lashHits;
        r.spoolHits = L.spoolHits;
        r.cuts = sim.loom37Cuts;
        r.severs = sim.loom37Severs;
        r.pulls = pilot.fightPulls;
        r.hpLeft = L.body.hp;
        r.tells = string.Join(" ", tellLog);
        r.legs = string.Join("; ", legLog);
        // every tell's wind-up within 1.0-1.5 s (rule 7)
        bool tellsOk = L.brain.tellLog.All(t => t.steps * dt >= 0.99 && t.steps * dt <= 1.51);
        r.ok = r.killed && owned && tellsOk;
        if (print)
        {
            Console.WriteLine($"fight seed {seed}: begin {began}, owned {owned}, {(r.killed ? "KILLED" : "NOT killed")} in {r.seconds:0.0} s (hp left {r.hpLeft:0}); tears pull {r.tearsPull} / LT {r.tearsLt}, windows {r.windows}, "
                + $"parries {r.parries}, lash hits {r.lashHits}, spool hits {r.spoolHits}, falls {r.falls}, deaths {r.deaths}, cuts {r.cuts}, severs {r.severs}, pulls {r.pulls}, still damage {r.still:0}/{r.dealt:0}");
            Console.WriteLine($"  tells ({r.yellow} YELLOW, {r.red} RED; every wind-up 1.0-1.5 s: {Pass37O(tellsOk)}): {r.tells}");
            Console.WriteLine($"  legs: {r.legs}");
            Console.WriteLine($"  fight: {Pass37O(r.ok)}");
        }
        return r;
    }

    static int Loom37Band(int seeds, double cap)
    {
        var runs = new List<Loom37Run>();
        for (int s = 1; s <= seeds; s++)
        {
            Loom37Run r = Loom37Fight((uint)s, cap, false);
            runs.Add(r);
            Console.WriteLine($"  seed {s}: {(r.killed ? "killed" : "NOT killed")} {r.seconds:0.0} s, tears {r.tearsPull}+{r.tearsLt}, parries {r.parries}, hits taken lash {r.lashHits} spool {r.spoolHits}, falls {r.falls}, deaths {r.deaths}, still {r.still:0}/{r.dealt:0}");
        }
        var t = runs.Where(r => r.killed).Select(r => r.seconds).OrderBy(x => x).ToList();
        double Q(double q) => t.Count == 0 ? double.NaN : t[Math.Min(t.Count - 1, (int)Math.Floor(q * (t.Count - 1) + 0.5))];
        double still = runs.Sum(r => r.still), dealt = runs.Sum(r => r.dealt);
        double share = dealt > 0 ? still / dealt : 0;
        bool all = t.Count == runs.Count;
        bool ok = all && Q(0.5) >= 15 && Q(0.5) <= 22 && Q(0.9) <= 26 && share < 0.05;
        Console.WriteLine($"band: {t.Count}/{runs.Count} killed, p10 {Q(0.1):0.0} s, p50 {Q(0.5):0.0} s, p90 {Q(0.9):0.0} s; damage while grounded and still {share * 100:0.0} % ({still:0} of {dealt:0}); {Pass37O(ok)}");
        return ok ? 0 : 1;
    }
}
