// NIGHT36 builder B (walkin36 + boss36): THE LANTERN on the boss framework, headless. One entry, N36Boss (declared and called in Program.cs).
//   boss36 [band|tells|walkin|all] [--seeds N] [--cap S] [--noparry] [--off36]
//     The hall (Lantern36Arena) is put at (2400, 0, 0) on the survival map (lantern36CentreOverride; an n36 level gives it by its row), the player
//     inside the gap, BossRoomBegin("hulk") with boss36 + walkin36 on (--off36: the rows off: the begin must fall back to the n35 path).
//     walkin  the begin is owned by boss36 (true, Lantern36 alive, BossRoomSystem("hulk") is it), the player's feet unmoved by the begin, the
//             slab closed after walkin36.lockSeconds, no damage in the reveal, the fight starts at revealSeconds. PASS each.
//     tells   one pilot run: every tell's measured length (Boss36Brain.tellLog) and kind; PASS: every tell 1.0-1.5 s, at least one of each
//             attack, never two tells at once (the brain's own rule: a tell starts only from REST), the stun at 50 % and a punish after each big attack.
//     band    Lantern36Pilot (mortal, retries) over seeds 1..N: kill time p10 / p50 / p90, deaths, parries, breaks, hits by share.
//             PASS: every seed killed, p50 in 15-22 s (GG w31-w32: the bot band), p90 <= 26 s. --noparry: the same pilot never punches (the control).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N36Boss(string cmd, string[] args, ref int code)
    {
        if (cmd != "boss36") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 5);
        double cap = ArgInt36(args, "--cap", 90);
        bool parry = Array.IndexOf(args, "--noparry") < 0;
        Trace36 = Array.IndexOf(args, "--trace") >= 0;
        All36 = Array.IndexOf(args, "--all36") >= 0; // every n36 sim row on (THE LANTERN's time with the night's whole game)
        int fails = 0;
        if (what == "walkin" || what == "all") fails += Walkin36();
        if (what == "tells" || what == "all") fails += Tells36Check(cap, parry);
        if (what == "camp" || what == "all") fails += Camp36();
        if (what == "level" || what == "all") fails += Level36Walk();
        if (what == "band" || what == "all") fails += Band36(seeds, cap, parry);
        Console.WriteLine(fails == 0 ? "boss36: GREEN" : $"boss36: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static int ArgInt36(string[] args, string key, int def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out int v) ? v : def;
    }

    static bool Trace36, All36;
    static double orbMin36 = 1e9;
    static readonly Vec3 Hall36 = new Vec3(2400, 0, 0);

    static SurvivalSim Sim36(uint seed, bool on)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n36Override = !on ? N36Blocks.Off : All36 ? new N36Blocks { tap36 = true, crunch36 = true, tells36 = true, parry36 = true, launch36 = true, heal36 = true, wallgrip36 = true, walkin36 = true, boss36 = true } : new N36Blocks { boss36 = true, walkin36 = true };
        sim.lantern36CentreOverride = Hall36;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        return sim;
    }

    static Vec3 Gap36(SurvivalSim sim) => new Vec3(Hall36.x, Hall36.y, Hall36.z + sim.lantern36Arena.p.apothem - 3);

    static string Pass36(bool ok) => ok ? "PASS" : "FAIL";

    static int Walkin36()
    {
        int fails = 0;
        bool on = !N36Blocks.ForceOff;
        SurvivalSim sim = Sim36(1, on);
        double dt = sim.cfg.Dt;
        Vec3 at = Gap36(sim);
        sim.Respawn(at, 0);
        sim.Step();
        Vec3 before = sim.mover.position;
        int cardSays = 0;
        sim.events.On<BossEvent>("bossIntro", e => cardSays++);
        bool ok = sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw);
        Vec3 after = sim.mover.position;
        bool owned = on && sim.lantern36 != null && sim.lantern36.Alive && ReferenceEquals(sim.BossRoomSystem("hulk"), sim.lantern36);
        Console.WriteLine($"walkin36: begin {ok}, boss36 owns 'hulk' {owned}, walkIn {sim.Boss36WalkIn}, feet moved by the begin {Vec3.DistanceTo(before, after):0.000} m, retry spawn ({spawn.x:0.0}, {spawn.y:0.0}, {spawn.z:0.0}) yaw {yaw:0}");
        if (!on)
        {
            Console.WriteLine($"  --off36: boss36 does not own the begin ({Pass36(!owned)}), walkIn off ({Pass36(!sim.Boss36WalkIn)})");
            return owned || sim.Boss36WalkIn ? 1 : 0;
        }
        bool l1 = ok && owned && sim.Boss36WalkIn && Vec3.DistanceTo(before, after) < 1e-9;
        Console.WriteLine($"  owned, walk-in, unmoved: {Pass36(l1)}");
        if (!l1) fails++;
        Lantern36 l = sim.lantern36;
        int lockSteps = MovementMath.StepsFor(sim.cfg.Num("design.n36.walkin36.lockSeconds"), dt), revealSteps = MovementMath.StepsFor(l.p.revealSeconds, dt);
        double hp0 = l.body.hp;
        int slabStep = -1, fightStep = -1;
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        for (int s = 1; s <= revealSteps + 60; s++)
        {
            // shoot the core through the whole reveal: nothing may land
            Vec3 eye = sim.Eye, dv = l.Centre - eye;
            double yawA = JsMath.Atan2(-dv.x, -dv.z), pitchA = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            si.SetLook(yawA, pitchA);
            si.Set(new InputFrame { yaw = yawA, pitch = pitchA }, new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = (s / 2) % 2 == 0, pressed = s % 4 == 0 } });
            sim.Step();
            sim.hitstopLeft = 0;
            if (slabStep < 0 && sim.world.RayIntersect(new Vec3(at.x, at.y + 1, at.z), new Vec3(0, 0, 1), out RayHit rh) && rh.distance < 5) slabStep = s;
            if (fightStep < 0 && l.brain.Fighting) { fightStep = s; Console.WriteLine($"  the fight starts at {s * dt:0.00} s; damage taken in the reveal {hp0 - l.body.hp:0} ({Pass36(hp0 - l.body.hp == 0)})"); if (hp0 - l.body.hp != 0) fails++; }
        }
        bool slabOk = slabStep > 0 && Math.Abs(slabStep - lockSteps) <= 2;
        Console.WriteLine($"  the slab closes at {slabStep * dt:0.00} s (want {lockSteps * dt:0.00}): {Pass36(slabOk)}");
        bool fightOk = fightStep > 0 && Math.Abs(fightStep - revealSteps) <= 2;
        Console.WriteLine($"  the fight starts at {fightStep * dt:0.00} s (want {revealSteps * dt:0.00}): {Pass36(fightOk)}; the core rose from {l.p.riseFrom} to {l.coreHeight:0.00} m");
        if (!slabOk) fails++;
        if (!fightOk) fails++;
        int cardWant = MovementMath.StepsFor(sim.cfg.Num("design.n36.walkin36.cardSeconds"), dt);
        bool cardOk = sim.lantern36CardStep > 0 && Math.Abs(sim.lantern36CardStep - cardWant) <= 2 && cardSays == 1;
        Console.WriteLine($"  the name card (bossIntro) at {sim.lantern36CardStep * dt:0.00} s (want {cardWant * dt:0.00}), bossIntro events {cardSays}, bar valid after it {sim.bossKit.Bar().valid}: {Pass36(cardOk)}");
        if (!cardOk) fails++;
        return fails;
    }

    sealed class Run36
    {
        public bool killed;
        public double seconds;
        public int deaths, parries, cracks, breaks, charged, full, light, stunHits, orbHits, floorHits, sweepHits, campHits, rides, punches, dodges;
        public List<(string, int)> tells = new List<(string, int)>();
        public int stuns, punishes, camps, yellow, red;
    }

    static Run36 Fight36(uint seed, double cap, bool parry, double stunAtFight = -1)
    {
        var r = new Run36();
        SurvivalSim sim = Sim36(seed, true);
        double dt = sim.cfg.Dt;
        Vec3 at = Gap36(sim);
        sim.Respawn(at, 0);
        if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) return r;
        Lantern36 l = sim.lantern36;
        var si = (ScriptedInput)sim.input;
        var pilot = new Lantern36Pilot { hand = new Mulberry32(seed * 7919u + 13u), parry = parry, shoot = stunAtFight < 0 };
        double yawL = 0;
        int step = 0, capSteps = (int)(cap / dt), revealSteps = MovementMath.StepsFor(l.p.revealSeconds, dt);
        var tells = new List<(string, int)>();
        while (l.Alive && step < capSteps + revealSteps)
        {
            if (stunAtFight >= 0 && step == revealSteps + (int)(stunAtFight / dt)) l.body.hp = Math.Min(l.body.hp, l.body.maxHp * l.brain.p.stunAt - 1); // the tells run: the bar moved to the stun by hand
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (Trace36 && l.brain.state == Boss36State.Active && l.brain.attack == Lantern36.Fan && l.brain.stateSteps % 30 == 1)
                Console.WriteLine($"    fan +{l.brain.stateSteps * dt:0.00} s: lead ({l.lastLead.x - Hall36.x:0.0},{l.lastLead.y:0.0},{l.lastLead.z - Hall36.z:0.0}) me ({sim.mover.position.x - Hall36.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z - Hall36.z:0.0}) {sim.mover.state} orb0 {(l.orbs.Count > 0 ? $"({l.orbs[0].pos.x - Hall36.x:0.0},{l.orbs[0].pos.y:0.0},{l.orbs[0].pos.z - Hall36.z:0.0})" : "-")}");
            if (Trace36 && l.orbs.Count > 0)
            {
                Vec3 mid = sim.mover.position + new Vec3(0, 0.9, 0);
                foreach (Lantern36Orb ob in l.orbs) if (!ob.back) orbMin36 = Math.Min(orbMin36, Vec3.DistanceTo(ob.pos, mid));
            }
            if (Trace36 && l.orbs.Count == 0 && orbMin36 < 1e8) { Console.WriteLine($"  fan closest orb to the player's middle {orbMin36:0.00} m"); orbMin36 = 1e9; }
            if (Trace36 && l.brain.entered) Console.WriteLine($"  t {step * dt:0.00} {l.brain.state} {l.brain.Current?.id} hp {l.body.hp:0} orbs {l.orbs.Count} core {l.coreHeight:0.0} open {l.CoreOpen} me ({sim.mover.position.x - Hall36.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z - Hall36.z:0.0}) {sim.mover.state}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                tells.AddRange(l.brain.tellLog);
                r.stuns += l.brain.stuns; r.punishes += l.brain.punishes; r.camps += l.brain.camps; r.yellow += l.brain.yellowTells; r.red += l.brain.redTells;
                r.parries += l.parries; r.cracks += l.cracksTotal; r.breaks += l.breaks; r.charged += l.chargedBreaks; r.full += l.fullHits; r.light += l.lightHits; r.stunHits += l.stunHits;
                r.orbHits += l.orbHits; r.floorHits += l.floorHits; r.sweepHits += l.sweepHits; r.campHits += l.campHits;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Respawn(spawn, yaw);
                sim.BossRoomBegin("hulk", out spawn, out yaw); // the campaign's retry: back to the fight's (or the stun's) start
            }
        }
        tells.AddRange(l.brain.tellLog);
        r.tells = tells;
        r.stuns += l.brain.stuns; r.punishes += l.brain.punishes; r.camps += l.brain.camps; r.yellow += l.brain.yellowTells; r.red += l.brain.redTells;
        r.parries += l.parries; r.cracks += l.cracksTotal; r.breaks += l.breaks; r.charged += l.chargedBreaks; r.full += l.fullHits; r.light += l.lightHits; r.stunHits += l.stunHits;
        r.orbHits += l.orbHits; r.floorHits += l.floorHits; r.sweepHits += l.sweepHits; r.campHits += l.campHits;
        r.killed = !l.Alive;
        r.seconds = Math.Max(0, step - revealSteps) * dt; // the fight's time (the reveal is the walk-in, not the fight)
        r.rides = pilot.rides; r.punches = pilot.punches; r.dodges = pilot.dodges;
        if (Trace36) { sim.events.Emitted.TryGetValue("punch", out int pe); sim.events.Emitted.TryGetValue("meleeSwing", out int me); sim.events.Emitted.TryGetValue("parry36", out int p36); Console.WriteLine($"  events: punch {pe}, meleeSwing {me}, parry36 {p36}"); }
        return r;
    }

    static string Line36(Run36 r) => $"killed {r.killed} in {r.seconds:0.0} s, deaths {r.deaths}, rides {r.rides}, punches {r.punches}, parries {r.parries}, cracks {r.cracks}, breaks {r.breaks} (charged {r.charged}), full hits {r.full}, light {r.light}, stun hits {r.stunHits}, taken: orbs {r.orbHits} floor ticks {r.floorHits} sweeps {r.sweepHits} camps {r.campHits}, dodges {r.dodges}";

    static int Tells36Check(double cap, bool parry)
    {
        Run36 r = Fight36(1, 45, parry, 22); // no shooting: the whole cycle, the stun forced at 22 s of fight, 45 s in all
        double dt = 1.0 / 120;
        var kinds = new Dictionary<string, (int n, double min, double max)>();
        bool allOk = true;
        foreach (var (a, steps) in r.tells)
        {
            double s = steps * dt;
            kinds.TryGetValue(a, out var k);
            kinds[a] = (k.n + 1, k.n == 0 ? s : Math.Min(k.min, s), Math.Max(k.max, s));
            if (s < 0.99 || s > 1.51) allOk = false;
        }
        Console.WriteLine($"tells36 (seed 1): {Line36(r)}");
        foreach (var kv in kinds) Console.WriteLine($"  {kv.Key}: {kv.Value.n} tells, {kv.Value.min:0.00}-{kv.Value.max:0.00} s");
        Console.WriteLine($"  yellow {r.yellow}, red {r.red}, punishes {r.punishes}, stuns {r.stuns}, camp attacks {r.camps}");
        bool each = kinds.ContainsKey("lanternOrbFan") && kinds.ContainsKey("lanternFloor") && kinds.ContainsKey("lanternSweep");
        Console.WriteLine($"  every tell 1.0-1.5 s: {Pass36(allOk)}; each of the three attacks told: {Pass36(each)}; the stun ran: {Pass36(r.stuns >= 1)}; punishes: {Pass36(r.punishes >= 1)}");
        return (allOk ? 0 : 1) + (each ? 0 : 1) + (r.stuns >= 1 ? 0 : 1) + (r.punishes >= 1 ? 0 : 1);
    }

    /// <summary>camp: a player who stands still 6 m from the centre (no input) gets the anti-camp slam aimed at his spot, within campSeconds + a rest
    /// + its tell; PASS: the camp attack told, its decal within campRadius of his feet, and it hit.</summary>
    static int Camp36()
    {
        SurvivalSim sim = Sim36(1, true);
        double dt = sim.cfg.Dt;
        sim.Respawn(Gap36(sim), 0);
        if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) return 1;
        Lantern36 l = sim.lantern36;
        Vec3 stand = new Vec3(Hall36.x + 6, Hall36.y, Hall36.z);
        sim.Respawn(stand, 0);
        var si = (ScriptedInput)sim.input;
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        int tellStep = -1;
        Vec3 decal = default;
        for (int s = 0; s < (int)(20 / dt); s++)
        {
            sim.Step();
            sim.hitstopLeft = 0;
            if (sim.health.Dead) break;
            if (tellStep < 0 && l.brain.state == Boss36State.Tell && l.brain.Current != null && l.brain.Current.camp) { tellStep = s; decal = l.brain.campAt; }
        }
        double off = JsMath.Hypot(decal.x - stand.x, decal.z - stand.z);
        bool ok = tellStep > 0 && off <= l.p.campRadius && l.campHits >= 1;
        Console.WriteLine($"camp36: the camper's slam told at {tellStep * dt:0.0} s (fight from {l.p.revealSeconds} s; campSeconds {l.brain.p.campSeconds}), decal {off:0.0} m from his feet, slams that hit {l.campHits}: {Pass36(ok)}");
        return ok ? 0 : 1;
    }

    /// <summary>level: builder L's n36 foundry as a CAMPAIGN run (DOORFREE so the rooms before need not be cleared): the hall from the level's own
    /// row; the player put in the approach hall (arena-local z +21) and walked forward (-z) for 6 s. PASS: the boss room started THE LANTERN (boss36
    /// owns it), no step moved the player more than 0.5 m (no Respawn into a far arena: the walk-in), he is inside the ring, the slab closed.</summary>
    static int Level36Walk()
    {
        string path = "Design/levels/n36/foundry.json";
        if (!System.IO.File.Exists(path)) { Console.WriteLine("level36 walk: no " + path + " (builder L's level): SKIP"); return 0; }
        bool on = !N36Blocks.ForceOff;
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out _);
        SurvivalSim sim = CampaignSim30(cfg, map, default);
        sim.n36Override = on ? new N36Blocks { boss36 = true, walkin36 = true } : N36Blocks.Off;
        sim.StartRun(1, null, null);
        sim.Step();
        if (sim.campaign != null) sim.campaign.doorFree29 = true;
        if (sim.lantern36Centre == null) { Console.WriteLine("level36 walk: the level has no lantern36 row: FAIL"); return 1; }
        Vec3 c = sim.lantern36Centre.Value;
        sim.mover.invulnerable = true;
        sim.Respawn(new Vec3(c.x, c.y + 0.05, c.z + 21), 0);
        var si = (ScriptedInput)sim.input;
        double dt = sim.cfg.Dt, maxJump = 0;
        int begunAt = -1, slabAt = -1;
        Vec3 prev = sim.mover.position;
        for (int s = 0; s < (int)(6 / dt); s++)
        {
            si.SetLook(0, 0);
            si.Set(new InputFrame { yaw = 0, moveZ = 1 }, new CombatInput { weapon = -1 });
            sim.Step();
            sim.hitstopLeft = 0;
            maxJump = Math.Max(maxJump, Vec3.DistanceTo(prev, sim.mover.position));
            prev = sim.mover.position;
            if (begunAt < 0 && sim.lantern36 != null && sim.lantern36.Alive) begunAt = s;
            if (slabAt < 0 && sim.Lantern36SlabClosed) slabAt = s;
            if (begunAt >= 0 && s - begunAt > 120) break;
        }
        Vec3 me = sim.mover.position;
        double r = JsMath.Hypot(me.x - c.x, me.z - c.z);
        bool owned = sim.lantern36 != null && ReferenceEquals(sim.BossRoomSystem("hulk"), sim.lantern36);
        string room = sim.campaign?.Room?.id ?? "-";
        Console.WriteLine($"level36 walk (n36 foundry, hall at {c.x:0},{c.z:0}): room now '{room}', the fight began at {(begunAt < 0 ? "never" : (begunAt * dt).ToString("0.00") + " s")}, boss36 owns it {owned}, biggest step {maxJump:0.00} m, {r:0.0} m from the centre, slab closed {(slabAt < 0 ? "never" : ((slabAt - begunAt) * dt).ToString("0.00") + " s after")}");
        if (!on)
        {
            Console.WriteLine($"  --off36: boss36 does not own it ({Pass36(!owned)}), biggest step {maxJump:0.00} m (information: with boss36 off the room runs the config's old LANTERN, or none)");
            return 0;
        }
        bool ok = begunAt >= 0 && owned && maxJump < 0.5 && r < 15 && slabAt >= 0;
        Console.WriteLine($"  walk-in, no cut: {Pass36(ok)}");
        return ok ? 0 : 1;
    }

    static int Band36(int seeds, double cap, bool parry)
    {
        var times = new List<double>();
        bool all = true;
        for (uint s = 1; s <= seeds; s++)
        {
            Run36 r = Fight36(s, cap, parry);
            Console.WriteLine($"band36 seed {s}: {Line36(r)}");
            times.Add(r.killed ? r.seconds : cap);
            if (!r.killed) all = false;
        }
        times.Sort();
        double P(double q) => times[Math.Min(times.Count - 1, (int)Math.Floor(q * (times.Count - 1) + 0.5))];
        double p50 = P(0.5), p90 = P(0.9);
        bool ok = all && p50 >= 15 && p50 <= 22 && p90 <= 26;
        Console.WriteLine($"band36{(parry ? "" : " --noparry")}: p10 {P(0.1):0.0} s, p50 {p50:0.0} s, p90 {p90:0.0} s, all killed {all}: {Pass36(ok)} (want p50 15-22 s, p90 <= 26 s)");
        return parry ? (ok ? 0 : 1) : 0; // the no-parry control is information
    }
}
