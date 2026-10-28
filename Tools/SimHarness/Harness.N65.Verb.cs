// NIGHT65 builder T (branch C's verb pilots; the lead's ask at 1:03): `n65verb`, scripted pilots on the real sim in a room's own geometry with
// the room's real waves, so a room's MAIN can show its verb working ("the verb cannot be skipped: not checked" gets a number). Modelled on
// Harness.N53.RideKill.cs (a CAMPAIGN run of the level file, DOORFREE, the player put in the room). Hooked without a Program.cs line (a module
// initializer, n46flow's / hold52's pattern). Each prints detail lines, then ONE verdict line `n65verb <verb>: GREEN|RED | <numbers>`.
//   n65verb launch|pierce|parry|skip --level <Design/levels/X.json> --room <id> [--pose x,y,z,yawDeg] [--seed n] [--off65] [...]
//   launch [--slide]   from the pose (default: the row's line65 first point, else the room's spawn), wave 1 live, invulnerable: the pilot walks
//                      at the nearest husk-shaped body, dashes (or, --slide, slides) into it and punches inside punch range: launch36 events,
//                      the flight's distance and end, bursts (hits, kills), where the body landed and whether it went into a pit / lava rect.
//   pierce [--yaw deg] the KESTREL charged to full from the pose (charge35Full) and released along the row's line65 first segment (or --yaw,
//                      else at the nearest body, stand-ins included): the charged shot's bodies hit (chargeShot27Fire), kills, stand-ins killed.
//   parry [--tries n]  from the pose, facing the nearest spitter: when one of its globs comes inside orb.reflectRange of the eye, a punch: parried
//                      (the glob reflected), the reflected glob's hit, the spitter killed, parry36's return burst (hits: a second victim).
//   skip [--refuse launch|pierce|parry|ride] [--seeds n] [--cap s]   the room cleared by the walk pilot (n46flow's: Walk38Pilot + the lava rule)
//                      twice per seed: the verb allowed, then the verb's input refused (launch: no punch while dashing / sliding / riding or in
//                      launch36's grace; pierce: fire held only on its press step, so no charge; parry: no punch with a glob inside 1.5 x
//                      reflectRange; ride: jump held only on its press step in the air). Clear time, deaths and the verb's uses, both runs.
//                      Default refusal by room: a2 / a8 pierce, a4 parry, a5 / b1 launch, a3 ride.   (NIGHT65's skip: now `skip --v1`)
//   skip [--seeds 10] [--bars <slate.json> | --bar b] [--refuse v] [--cap s] [--plant-verb65 r:v] [--plant-forced | --plant-null]
//                      NIGHT66 T1b, SKIP V2 (V66SkipV2): three runs per seed, the verb-aware pilot, the same pilot with the verb refused
//                      inside it (episodes declined, holds and approach released, so it plays on), the plain walker; the paired ratio
//                      min(refused-in, walker) / verb-aware per seed; the verdict `n65verb skip: M1 PASS|FAIL|UNSURE | room <id>, ratio <x>
//                      vs bar <b>, exercised <n>/<seeds>` (7 of 10 needed), then M2 (STYLE), the touch table and launch36's kills
// A pose inside a collision box is refused (Tour65.InsideSolid). A room with no target prints RED with its reason. Probe numbers only: not gameplay.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void V65Init()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n65verb") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string verb = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "";
            code = verb == "launch" ? V65Launch(args) : verb == "pierce" ? V65Pierce(args) : verb == "parry" ? V65Parry(args) : verb == "skip" ? (Array.IndexOf(args, "--v1") >= 0 ? V65SkipV1(args) : V66SkipV2(args)) : verb == "ride" ? V66Ride(args) : 2;
            if (code == 2) Console.WriteLine("n65verb: usage: n65verb launch|pierce|parry|ride|skip --level <file> --room <id> [--pose x,y,z,yawDeg] [--seed n] [--off65] (ride: [--shoot] [--plant-target auto|x,y,z] [--face k])");
        }
        catch (Exception e) { Console.WriteLine("n65verb: EXCEPTION " + e); code = 2; }
        // NIGHT66 T1b amendment (builder S: skip v2 sat idle after printing everything, a8 with stand-ins): the output flushed, a closing line
        // (so a reader sees the command ended), then the exit. Why the exit stalled is NOT established; n65-verbs.sh runs each call under
        // `timeout 900` so a stall ends with the output kept
        Console.WriteLine($"n65verb: exit {code}");
        Console.Out.Flush(); Console.Error.Flush();
        Environment.Exit(code);
    }

    static readonly CultureInfo V65Ci = CultureInfo.InvariantCulture;
    const double V65WaveWait = 15; // s of sim time for the room's first live enemy (a8's wave 1 takes over 8 s: its lesson gates it)
    static string V65F(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "-" : v.ToString("0.0", V65Ci);
    static string V65P(Vec3 p) => $"({V65F(p.x)}, {V65F(p.y)}, {V65F(p.z)})";

    /// <summary>One probe's sim: the level as a CAMPAIGN run (the config's own rows, as n46flow plays it), the player put at the pose inside the
    /// room by DOORFREE, invulnerable, the room's fight running (DOORFREE off once the campaign holds the room).</summary>
    sealed class V65Run
    {
        public SurvivalSim sim;
        public LevelDef level;
        public RoomDef rd;
        public int room;
        public Dictionary<string, object> json, row;
        public Vec3 pose;
        public double yawDeg;
        public string poseFrom, refused;
        public List<double[]> solids;
        public bool god = true;

        public ScriptedInput In => (ScriptedInput)sim.input;

        public void Step(InputFrame f, CombatInput c)
        {
            In.SetLook(f.yaw, f.pitch);
            In.Set(f, c);
            sim.hitstopLeft = 0;
            if (god) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; }
            sim.Step();
        }

        public IEnumerable<Enemy> Alive => sim.enemies.all.Where(e => e.alive && e.position.x >= rd.x0 - 1 && e.position.x <= rd.x1 + 1 && e.position.z >= rd.z0 - 1 && e.position.z <= rd.z1 + 1);
    }

    static V65Run V65Start(string[] args, string verb)
    {
        string path = N53RkOpt(args, "--level", "Design/levels/n52-foundry.json");
        string roomId = N53RkOpt(args, "--room", "a5");
        uint seed = uint.Parse(N53RkOpt(args, "--seed", "1"), V65Ci);
        GameConfig cfg = Config();
        var r = new V65Run();
        SurvivalMap map = CampaignMapFrom(path, cfg, out r.json);
        r.level = LevelDef.From(r.json);
        r.room = r.level.rooms.FindIndex(x => x.id == roomId);
        if (r.room < 0) { r.refused = $"no room {roomId} in {path}"; return r; }
        r.rd = r.level.rooms[r.room];
        foreach (object o in PortData.Arr(PortData.Obj(r.json["campaign"])["rooms"])) { var d = PortData.Obj(o); if ((string)d["id"] == roomId) r.row = d; }
        r.solids = Tour65.Solids(r.json);
        // the pose: --pose, else the row's line65 first point (yaw along its first segment), else the room's spawn
        string poseArg = N53RkOpt(args, "--pose", null);
        List<double[]> line = Tour65.Line(r.row);
        if (poseArg != null)
        {
            double[] v = poseArg.Split(',').Select(s => double.Parse(s, V65Ci)).ToArray();
            r.pose = new Vec3(v[0], v[1], v[2]); r.yawDeg = v.Length > 3 ? v[3] : r.rd.spawnYawDeg; r.poseFrom = "--pose";
        }
        else if (line.Count >= 2)
        {
            r.pose = new Vec3(line[0][0], line[0][1], line[0][2]);
            r.yawDeg = Math.Atan2(-(line[1][0] - line[0][0]), -(line[1][2] - line[0][2])) * 180 / Math.PI; r.poseFrom = "line65[0]";
        }
        else { r.pose = r.rd.spawn; r.yawDeg = r.rd.spawnYawDeg; r.poseFrom = "the room's spawn (no line65)"; }
        string inside = Tour65.InsideSolid(r.solids, new Tour65.Pose { x = r.pose.x, y = r.pose.y, z = r.pose.z });
        if (inside != null) { r.refused = $"the pose {V65P(r.pose)} ({r.poseFrom}) is inside a solid ({inside}): refused"; return r; }
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        r.sim = SurvivalSim.Headless(cfg, map);
        r.sim.noRecords = r.sim.noLab = true;
        r.sim.setup.mode = Campaign24.Mode;
        r.sim.StartRun(seed, null, null);
        if (r.sim.campaignLevel == null) { r.refused = "the run did not start as a CAMPAIGN"; return r; }
        // the reel's placement (n46flow's): DOORFREE, the player at the pose; DOORFREE off once the campaign holds the room
        var idle = new InputFrame { yaw = r.yawDeg * Math.PI / 180 };
        for (int s = 0; s < 240 && (r.sim.campaign == null || r.sim.campaign.room < 0); s++) r.Step(idle, new CombatInput { weapon = -1 });
        r.sim.campaign?.SetDoorFree29(true);
        r.sim.Respawn(r.pose, r.yawDeg);
        // NIGHT67 T1c (the lead: R's g7 parry M3 RED, one punch, the gauntlet in hand): the verb pilots' put-in gets the awards of the rooms
        // before this one, as skip v2 and n46flow do (V67PutInAwards), right after the Respawn and BEFORE the room is entered, so the room's
        // own award (the game's, on entry: kb3's gauntlet) comes last as in the game; --no-put-in-awards keeps the old put-in (the planted RED)
        V67PutInOff = Array.IndexOf(args, "--no-put-in-awards") >= 0;
        V67PutInAwards(r.sim, r.level, r.room);
        for (int s = 0; s < 360 && r.sim.campaign.room != r.room; s++) r.Step(idle, new CombatInput { weapon = -1 });
        if (r.sim.campaign.room != r.room) { r.refused = $"the campaign never took {roomId} (it holds {(r.sim.campaign.room >= 0 ? r.level.rooms[r.sim.campaign.room].id : "none")})"; return r; }
        r.sim.campaign.SetDoorFree29(false);
        Console.WriteLine($"  entry loadout (pilot, {roomId}, on entering): owned {string.Join(" ", r.sim.loadout.owned)}, in hand {r.sim.loadout.Weapon}, pending {V67Pending(r.sim)}"); // NIGHT67 T1c: a print only
        Console.WriteLine($"n65verb {verb}: level {r.level.id} ({path}), room {roomId} \"{r.rd.name}\", seed {seed}; pose {V65P(r.pose)} yaw {V65F(r.yawDeg)} from {r.poseFrom}; off switch {(NightRows.ForceOffFrom == NightRows.NoOff ? "none" : "--off" + NightRows.ForceOffFrom)}");
        return r;
    }

    /// <summary>Waits (sim time) for the room's first live enemy, then `settle` s more (the wave out of its spawn tell). False: none came.</summary>
    static bool V65Wave(V65Run r, double wait, double settle, InputFrame f)
    {
        double t0 = r.sim.SimTime;
        while (!r.Alive.Any() && r.sim.SimTime - t0 < wait) r.Step(f, new CombatInput { weapon = -1 });
        if (!r.Alive.Any()) return false;
        double t1 = r.sim.SimTime;
        while (r.sim.SimTime - t1 < settle) r.Step(f, new CombatInput { weapon = -1 });
        return r.Alive.Any();
    }

    static double V65Yaw(Vec3 from, Vec3 to) => Math.Atan2(-(to.x - from.x), -(to.z - from.z));
    static double V65Pitch(Vec3 from, Vec3 to) => Math.Atan2(to.y - from.y, Math.Sqrt((to.x - from.x) * (to.x - from.x) + (to.z - from.z) * (to.z - from.z)));
    static double V65Flat(Vec3 a, Vec3 b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));

    /// <summary>The level's pit rects of this room ([x0, z0, x1, z1, floorY, lava 0/1]) and the level's lava rects.</summary>
    static List<double[]> V65Pits(V65Run r)
    {
        var res = new List<double[]>();
        var camp = PortData.Obj(r.json["campaign"]);
        if (camp.TryGetValue("pits", out object po))
            foreach (object o in PortData.Arr(po))
            {
                var d = PortData.Obj(o);
                if (d.TryGetValue("zone", out object z) && (string)z != r.rd.id) continue;
                var rc = PortData.Arr(d["rect"]);
                bool lava = d.TryGetValue("lava", out object lv) && lv is bool b && b;
                res.Add(new[] { PortData.Num(rc[0]), PortData.Num(rc[1]), PortData.Num(rc[2]), PortData.Num(rc[3]), d.TryGetValue("floorY", out object fy) ? PortData.Num(fy) : -1, lava ? 1 : 0 });
            }
        return res;
    }

    // ================================================================ launch
    static int V65Launch(string[] args)
    {
        V65Run r = V65Start(args, "launch");
        if (r.refused != null) { Console.WriteLine($"n65verb launch: RED | {r.refused}"); return 1; }
        SurvivalSim sim = r.sim;
        bool slide = Array.IndexOf(args, "--slide") >= 0;
        var look = new InputFrame { yaw = r.yawDeg * Math.PI / 180 };
        double runMaxPunch = 0; // NIGHT68 I2: the largest non-parry punch dealt in this run (the launch bound's second term; a parry never launches)
        bool runParry = false;
        Action offPm = sim.events.On<MeleeEvent>("punch", e => runParry = e.parry);
        Action offP = sim.events.On<HitEvent>("hit", e => { if (e.weapon == "punch" && !runParry && e.damage > runMaxPunch) runMaxPunch = e.damage; });
        if (!V65Wave(r, V65WaveWait, 1.0, look)) { Console.WriteLine($"n65verb launch: RED | no target: no live enemy in {r.rd.id} within {V65WaveWait} s of the pose"); return 1; }
        Launch36 lc = sim.launch36Core;
        if (lc == null) { Console.WriteLine("n65verb launch: RED | launch36 is off in this config (sim.launch36Core null)"); return 1; }
        // NIGHT65 (the GATE critic, Logs/n65/verdict-a5-r2.md): --wave n plays the room up to its wave n (the earlier waves killed outright
        // after 2 s, as parry does; the campaign spawns wave n at its own nodes), --target x,y,z takes the husk nearest that point
        int wantWave = int.Parse(N53RkOpt(args, "--wave", "1"), V65Ci);
        string tgtArg = N53RkOpt(args, "--target", null);
        if (wantWave > 1)
        {
            double tw = sim.SimTime, quiet = sim.SimTime;
            int forced = 0;
            while (sim.campaign.wave < wantWave && sim.SimTime - tw < 60)
            {
                if (sim.SimTime - quiet > 2 && r.Alive.Any(e => !e.standIn29)) { foreach (Enemy e in r.Alive.Where(e => !e.standIn29).ToList()) { sim.loadout.Damage(e, 1e6, false, "revolver", sim.Eye); forced++; } quiet = sim.SimTime; }
                r.Step(look, new CombatInput { weapon = -1 });
            }
            if (sim.campaign.wave < wantWave) { Console.WriteLine($"n65verb launch: RED | no target: the room never reached wave {wantWave} in 60 s (waves spawned {sim.campaign.wave}; {forced} bodies killed outright)"); return 1; }
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < 1.0 && !r.Alive.Any(e => e is Husk && !e.standIn29)) r.Step(look, new CombatInput { weapon = -1 });
            for (double t2 = sim.SimTime; sim.SimTime - t2 < 1.0;) r.Step(look, new CombatInput { weapon = -1 });
            Console.WriteLine($"  wave {wantWave} reached (waves spawned {sim.campaign.wave}); {forced} bodies of earlier waves killed outright");
        }
        Vec3 near = sim.mover.position;
        if (tgtArg != null) { double[] v = tgtArg.Split(',').Select(s => double.Parse(s, V65Ci)).ToArray(); near = new Vec3(v[0], v[1], v[2]); }
        // NIGHT67 T1c (the lead: kb3's punch launched nothing, v2's dash-punch killed the husk outright): (1) the gauntlet in hand drops the
        // punch (Rules30.MeleeBlocked): a player switches to a gun to punch, so the pilot does (the best owned gun), or says there is none;
        // (2) launch36 throws only a body the punch leaves ALIVE (Combat.cs:1406-1410: Damage, then `if (launch && enemy.alive) Launch`), so the
        // target is the nearest husk-shaped body (Launch36.Wants: a Husk, no stand-in, no tank) whose hp is above punch.damage, or the reason
        // none qualifies is printed; (3) the dash starts within punch.range + 2 m, so the punch lands inside the dash or its grace.
        // `--launch-v1` keeps NIGHT65's pilot (the nearest husk, the hand as it is, the dash at 5.5 m): the planted RED.
        bool v1 = Array.IndexOf(args, "--launch-v1") >= 0;
        double pDmg = sim.cfg.Num("punch.damage");
        if (!v1 && sim.loadout.Weapon == "gauntlet")
        {
            string gun = new[] { "revolver", "shotgun" }.FirstOrDefault(w => sim.loadout.owned.Contains(w)) ?? sim.loadout.owned.FirstOrDefault(w => w != "gauntlet");
            if (gun == null) { Console.WriteLine($"n65verb launch: RED | room {r.rd.id} | the gauntlet is in hand and the loadout holds no gun to switch to (owned: {string.Join(" ", sim.loadout.owned)}): the punch is dropped with the gauntlet in hand (Rules30.MeleeBlocked)"); return 1; }
            int slot = sim.loadout.owned.IndexOf(gun);
            for (int i = 0; i < 360 && (sim.loadout.Weapon != gun || i < 24); i++) r.Step(look, new CombatInput { weapon = slot });
            Console.WriteLine($"  the hand: the gauntlet in hand drops the punch, so the pilot switched to {gun} (in hand {sim.loadout.Weapon}), as a player does to punch");
        }
        Func<Enemy, bool> throwable = e => e is Husk && !e.standIn29 && !e.tank29;
        // NIGHT68 I2: `--plant-pair` (the planted pair; probe values, never gameplay): the room's own bodies killed outright, then two husks held
        // in front of the player through their spawn tell, A at 3 m with hp 11 and B at 6 m with hp 400; the target is chosen between the two
        List<Enemy> pool = null;
        if (Array.IndexOf(args, "--plant-pair") >= 0)
        {
            int killed0 = 0;
            foreach (Enemy e in r.Alive.Where(e => !e.standIn29).ToList()) { sim.loadout.Damage(e, 1e6, false, "revolver", sim.Eye); killed0++; }
            Vec3 p0 = sim.mover.position;
            double ly = r.yawDeg * Math.PI / 180, fx = -Math.Sin(ly), fz = -Math.Cos(ly);
            Vec3 sa = new Vec3(p0.x + fx * 3, p0.y, p0.z + fz * 3), sb = new Vec3(p0.x + fx * 6, p0.y, p0.z + fz * 6);
            Husk ha = sim.husks.Spawn("husk", sa, p0), hb = sim.husks.Spawn("husk", sb, p0);
            if (ha == null || hb == null) { Console.WriteLine($"n65verb launch: RED | --plant-pair: a planted husk did not spawn (A at {V65P(sa)} {(ha == null ? "NO" : "yes")}, B at {V65P(sb)} {(hb == null ? "NO" : "yes")})"); return 1; }
            ha.hp = 11; hb.hp = 400;
            for (int i = 0; i < 120; i++) { ha.position = ha.previous = sa; hb.position = hb.previous = sb; ha.cooldownSteps = hb.cooldownSteps = Math.Max(hb.cooldownSteps, 600); r.Step(look, new CombatInput { weapon = -1 }); }
            ha.hp = 11; hb.hp = 400;
            pool = new List<Enemy> { ha, hb };
            Console.WriteLine($"  planted pair: {killed0} room bodies killed outright; husk A #{ha.id} at {V65P(sa)} hp {V65F(ha.hp)}, husk B #{hb.id} at {V65P(sb)} hp {V65F(hb.hp)} (3 m and 6 m along the look); their attacks held off (cooldown kept at 600 steps or more), so a punch on them is never parry36's early parry (Parry36.cs Imminent36), which never launches");
        }
        IEnumerable<Enemy> cands = pool != null ? pool.Where(e => e.alive) : r.Alive;
        Enemy target;
        if (v1) target = cands.Where(throwable).OrderBy(e => V65Flat(e.position, near)).FirstOrDefault();
        else
        {
            double floorY = r.rd.spawn.y; // a burrowed body (more than 1 m under the room's floor, kb2c's burrower at y -6) is no target
            // NIGHT68 I2: the bound is max(punch.damage x momentum.meleeMax, the largest non-parry punch dealt in this run) (the punch is x
            // lerp(1, meleeMax, momentum), then the arsenal's tags, the bonus and vuln26: Combat.cs:1483-1511; shipped kb3's dash punches on 40 hp
            // husks, seeds 1-3: 24.4 survived and launched, 48.7 and 43.1 killed and were parries, which never launch); NIGHT67 T1c's rule was
            // hp > punch.damage (10)
            double baseB = pDmg * sim.cfg.Num("momentum.meleeMax"), bound = Math.Max(baseB, runMaxPunch);
            string bnd = $"the launch bound {V65F(bound)} (max(punch.damage {V65F(pDmg)} x momentum.meleeMax {V65F(sim.cfg.Num("momentum.meleeMax"))} = {V65F(baseB)}, the largest non-parry punch in this run {V65F(runMaxPunch)}))";
            // NIGHT68 I2 fix round (verdict-I2.md: kb3's 40 hp husks died to PARRY punches): a body whose punch would be a parry now (winding up,
            // or inside parry36's early window: V68ParryWhy, Harness.N68.Parry.cs) is skipped as a target, the reason printed
            target = cands.Where(e => throwable(e) && e.hp > bound && e.position.y >= floorY - 1.0 && V68ParryWhy(sim, e) == null).OrderBy(e => V65Flat(e.position, near)).FirstOrDefault();
            if (target == null) { Console.WriteLine($"n65verb launch: RED | room {r.rd.id} | no launch target: every husk-shaped body alive has hp at or under {bnd} (the punch would kill it before launch36 can throw it) or would be parried now, or none is alive (alive: {string.Join(", ", cands.Select(e => e.kind + " hp " + V65F(e.hp) + (e.standIn29 ? " (stand-in)" : e.tank29 ? " (tank)" : "") + (V68ParryWhy(sim, e) is string pw0 ? " (" + pw0 + ")" : "")))})"); return 1; }
            Console.WriteLine($"  target chosen: {target.kind} #{target.id} hp {V65F(target.hp)} > {bnd} (the nearest husk-shaped body the punch leaves alive, so launch36 can throw it), {V65F(V65Flat(target.position, near))} m off");
            foreach (Enemy e in cands.Where(e => throwable(e) && e.hp <= bound && e.position.y >= floorY - 1.0)) Console.WriteLine($"  skipped: {e.kind} #{e.id} hp {V65F(e.hp)} (at or under the bound: the punch would kill it)");
            foreach (Enemy e in cands.Where(e => throwable(e) && e.hp > bound && e.position.y >= floorY - 1.0 && V68ParryWhy(sim, e) != null)) Console.WriteLine($"  skipped: {e.kind} #{e.id} hp {V65F(e.hp)} (its punch would be a parry now: {V68ParryWhy(sim, e)}; a parry never launches)");
        }
        if (target != null && (wantWave > 1 || tgtArg != null)) Console.WriteLine($"  target chosen: {target.kind} #{target.id} at {V65P(target.position)} (nearest to {(tgtArg != null ? "--target " + V65P(near) : "the pose")}, wave {sim.campaign.wave})");
        if (target == null) { Console.WriteLine($"n65verb launch: RED | no target: no husk-shaped body alive in {r.rd.id} that launch36 can throw (alive: {string.Join(", ", r.Alive.Select(e => e.kind + (e.standIn29 ? " stand-in" : e.tank29 ? " tank" : "")))}; stand-ins and tanks never fly)"); return 1; }
        int launches0 = lc.launches, bursts0 = lc.bursts, burstHits0 = lc.burstHits, burstKills0 = lc.burstKills;
        int launchEv = 0, burstEv = 0;
        var burstAt = new List<string>();
        // NIGHT69 Y2 (slate Y2; scout.md X1): the FIRST launched body (launch36's LaunchEvent carries its id, Launch36.cs:133), its object taken
        // at the event (enemies are pooled: the id is checked again on every tracked step), its position then, and its own death
        int launchedId = -1;
        Enemy launchedBody = null;
        Vec3 lbFrom = default, lbDeathAt = default;
        bool lbDead = false;
        Action offL = sim.events.On(Launch36.LaunchEvent, o =>
        {
            launchEv++;
            if (launchedId < 0 && o is int lid) { launchedId = lid; launchedBody = sim.enemies.all.FirstOrDefault(e => e.id == lid && e.alive); if (launchedBody != null) lbFrom = launchedBody.position; }
        });
        Action offK2 = sim.events.On<KillEvent>("kill", e => { if (launchedId >= 0 && e.enemy == launchedId && !lbDead) { lbDead = true; lbDeathAt = e.position; } });
        Action offB = sim.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => { burstEv++; burstAt.Add($"{V65P(e.position)} hits {e.hits} kills {e.kills}{(e.wall ? " (a wall)" : "")}"); });
        string killedBy = null;
        bool dead = false; // enemies are pooled: once the target dies its object may come back as a new spawn, so the track stops at the death
        Vec3 deathAt = default;
        Action offK = sim.events.On<KillEvent>("kill", e => { if (e.enemy == target.id && !dead) { killedBy = e.weapon; dead = true; deathAt = e.position; } });
        // NIGHT68 I2: what the punch dealt on its target (the HitEvent of weapon "punch"; Combat.cs: Damage, then launch36 only if it lives)
        double dealt = double.NaN, hpBefore = target.hp;
        bool punchKilled = false;
        Action offH = sim.events.On<HitEvent>("hit", e => { if (e.weapon == "punch" && e.enemy == target.id && double.IsNaN(dealt)) { dealt = e.damage; punchKilled = e.killed; } });
        string punchParry = null; // NIGHT68 I2: the punch's MeleeEvent: a parry (the body winding up, or parry36's early buffer) never launches (Combat.cs: launch = !parry && Wants)
        Action offM = sim.events.On<MeleeEvent>("punch", e => { if (punchParry == null) punchParry = e.parry ? "a PARRY (no launch: the body was winding up or about to)" : "not a parry"; });
        if (sim.mover.stamina.rules.Any) sim.mover.stamina.bar = 1;
        double pRange = sim.cfg.Num("punch.range"), closest = double.MaxValue;
        Vec3 from = sim.mover.position;
        int t = 0, dashAt = -1, redashes = 0;
        int parryHeld = 0; string heldWhy = null; // NIGHT68 I2 fix round: steps the punch was held because it would be a parry
        bool punched = false;
        string stateAtPunch = "";
        // approach: walk at it; inside 5.5 m flat a dash (or a held slide); a punch once inside 0.9 x punch range (the launch36 reel's rule)
        for (; t < MovementMath.StepsFor(10, sim.step) && target.alive && !punched; t++)
        {
            if (pool != null) foreach (Enemy pe in pool) if (pe.alive && pe is Husk ph) ph.cooldownSteps = Math.Max(ph.cooldownSteps, 600); // NIGHT68 I2: the planted pair's attacks held off
            Vec3 eye = sim.Eye, at = target.AimPoint;
            double flat = V65Flat(target.position, sim.mover.position);
            closest = Math.Min(closest, flat);
            var f = new InputFrame { yaw = V65Yaw(eye, at), pitch = V65Pitch(eye, at), moveZ = 1 };
            // NIGHT67 T1c: a dash whose speed window ended (no Dash, no launch36 grace) before the body came into reach is spent: re-approach and
            // dash again (a punch after the window launches nothing: Launch36.Wants); the first dash that reaches plays as before
            if (!v1 && !slide && dashAt >= 0 && t - dashAt > 2 && sim.mover.state != MoveState.Dash && lc.graceLeft <= 0 && flat > pRange * 0.9) { dashAt = -1; redashes++; }
            if (!slide && flat < 5.5 && dashAt < 0) { f.dash = new Button { held = true, pressed = true }; dashAt = t; }
            else if (!slide && dashAt >= 0 && t - dashAt < 2) f.dash = new Button { held = true };
            if (slide && flat < 6.5) { f.slide = new Button { held = true, pressed = dashAt < 0 }; if (dashAt < 0) dashAt = t; }
            if (flat <= pRange * 0.9 && (dashAt >= 0))
            {
                // NIGHT68 I2 fix round: the punch held while it would be a parry (the target winding up, or inside parry36's early window: a
                // parry never launches, and kb3's parry punches killed the 40 hp husks); once the window closes, a spent speed window is dashed
                // again so the punch lands at speed (Launch36.Wants)
                string pw = v1 ? null : V68PunchParryWhy(sim, target, f.yaw, f.pitch);
                if (pw != null) { parryHeld++; heldWhy = pw; f.moveZ = 0; f.dash = default; r.Step(f, new CombatInput { weapon = -1 }); continue; }
                bool speedNow = sim.mover.state == MoveState.Dash || sim.mover.state == MoveState.Slide || lc.graceLeft > 0;
                // not at speed yet (the dash pressed this step starts on the sim's step) or no longer: wait for it, or dash again once spent
                if (!v1 && !speedNow) { if (t - dashAt > 2) { dashAt = -1; redashes++; } r.Step(f, new CombatInput { weapon = -1 }); continue; }
                stateAtPunch = sim.mover.state + (lc.graceLeft > 0 ? " (grace " + lc.graceLeft + ")" : "") + $", {V65F(flat)} m off (reach {V65F(pRange)}), launch36 wants it {(lc.Wants(sim.mover, target) ? "yes" : "NO")}, its hp {V65F(target.hp)} vs the punch's {V65F(pDmg)}, in hand {sim.loadout.Weapon}" + (redashes > 0 ? $", after {redashes} spent dash(es)" : ""); // NIGHT67 T1c: why a punch throws or not
                r.Step(f, new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } });
                punched = true;
                break;
            }
            r.Step(f, new CombatInput { weapon = -1 });
        }
        // the flight: the body tracked 3 s (its end, how far, where it came down, whether it went into a pit or the lava)
        // NIGHT69 Y2 (slate Y2; scout.md X1; verdict-I2-r2.md: kb3's RED probe printed "in a pit YES" for a target nothing threw, and the readers
        // passed M3): the body tracked is the LAUNCHED one (launch36's first LaunchEvent id; it is the target on a plain throw, another body when
        // the punch threw another), and in a pit / in lava are set only from it, only once launch36 fired (launchEv >= 1); with nothing launched
        // the target is tracked for `landed` and `max` only, and the pit and the lava read n/a
        Vec3 targetAt = target.position; // (the target after the punch's step: as before Y2, printed on the target line)
        Vec3 launchedFrom = launchedBody != null ? launchedBody.position : target.position, landed = launchedFrom; // (the same moment as before Y2: after the punch's step)
        bool inPit = false, inLava = false;
        var pits = V65Pits(r);
        double maxThrown = 0;
        bool Flying() => launchEv >= 1 && launchedBody != null;
        bool TrackDead() => Flying() ? lbDead : dead;
        bool wasFlying = Flying();
        for (int k = 0; k < MovementMath.StepsFor(3, sim.step) && !TrackDead(); k++)
        {
            r.Step(new InputFrame { yaw = r.In.Yaw }, new CombatInput { weapon = -1 });
            bool fl = Flying();
            if (fl && !wasFlying) { launchedFrom = lbFrom; maxThrown = 0; wasFlying = true; } // a launch after the punch's step: its flight from there
            Enemy tb = fl ? launchedBody : target;
            bool tdead = TrackDead();
            Vec3 at = tdead ? (fl ? lbDeathAt : deathAt) : tb.position;
            if (!tdead && (!tb.alive || tb.id != (fl ? launchedId : target.id))) break; // gone with no kill event (a pit's end, the lava, the pool): its last position stands
            landed = at;
            maxThrown = Math.Max(maxThrown, V65Flat(landed, launchedFrom));
            if (fl)
                foreach (double[] p in pits)
                    if (landed.x > p[0] && landed.x < p[2] && landed.z > p[1] && landed.z < p[3] && landed.y < -0.2) { if (p[5] > 0) inLava = true; else inPit = true; }
        }
        if (TrackDead()) { landed = Flying() ? lbDeathAt : deathAt; maxThrown = Math.Max(maxThrown, V65Flat(landed, launchedFrom)); }
        bool flew = Flying();
        string lbText = launchEv < 1 ? "launched body none" : launchedBody == null ? $"launched body #{launchedId} (not found alive at the event: not tracked)" : $"launched body {launchedBody.kind} #{launchedId} (the target: {(launchedId == target.id ? "yes" : "no")})";
        string pitText = flew ? $"in a pit {(inPit ? "YES" : "no")}, in lava {(inLava ? "YES" : "no")}" : "in a pit n/a (nothing launched), in lava n/a (nothing launched)";
        offL(); offB(); offK(); offH(); offP(); offPm();
        offM(); offK2();
        Console.WriteLine($"  punch: {(double.IsNaN(dealt) ? "no punch landed on the target" : $"dealt {V65F(dealt)} vs hp {V65F(hpBefore)}: {(punchKilled ? "KILLED" : "survived")}")} | target {target.kind} hp {V65F(hpBefore)} | the punch {punchParry ?? "not thrown"} | held {parryHeld} steps while it would be a parry{(heldWhy != null ? " (" + heldWhy + ")" : "")}, dashes re-made {redashes}");
        foreach (string b in burstAt) Console.WriteLine("  burst at " + b);
        Console.WriteLine($"  target {target.kind} #{target.id} at {V65P(targetAt)} (from {V65P(from)}), {(slide ? "slide" : "dash")} at step {dashAt}, punch {(punched ? $"at step {t} in state {stateAtPunch}" : $"NOT thrown (closest {V65F(closest)} m, range {V65F(pRange)} m)")}");
        bool green = punched && launchEv >= 1 && lc.launches > launches0 && (lc.lastThrown >= 1.0 || maxThrown >= 1.0 || lc.bursts > bursts0);
        Console.WriteLine($"n65verb launch: {(green ? "GREEN" : "RED")} | room {r.rd.id} | launch36 {launchEv}, flight end {lc.lastEnd} thrown {V65F(lc.lastThrown)} m (max {V65F(maxThrown)} m), bursts {lc.bursts - bursts0} (hits {lc.burstHits - burstHits0}, kills {lc.burstKills - burstKills0})"
            + $" | {lbText}, landed {V65P(landed)}{(flew ? "" : " (the target: nothing launched)")}, {pitText}, target {(dead ? "dead (" + (killedBy ?? "?") + ")" : target.alive ? "alive" : "gone")} | pose {r.poseFrom}{(green ? "" : punched ? " | the punch launched nothing far" : " | no punch")}");
        return green ? 0 : 1;
    }

    // ================================================================ pierce
    static int V65Pierce(string[] args)
    {
        V65Run r = V65Start(args, "pierce");
        if (r.refused != null) { Console.WriteLine($"n65verb pierce: RED | {r.refused}"); return 1; }
        SurvivalSim sim = r.sim;
        // the KESTREL in hand (n53ridekill's Equip)
        if (sim.loadout.owned.IndexOf("revolver") < 0) sim.loadout.AddWeapon("revolver");
        int slot = sim.loadout.owned.IndexOf("revolver");
        var look = new InputFrame { yaw = r.yawDeg * Math.PI / 180 };
        for (int i = 0; i < 360 && (sim.loadout.Weapon != "revolver" || i < 24); i++) r.Step(look, new CombatInput { weapon = slot });
        if (!V65Wave(r, V65WaveWait, 1.0, look)) { Console.WriteLine($"n65verb pierce: RED | no target: no live enemy in {r.rd.id} within {V65WaveWait} s of the pose"); return 1; }
        // the aim: --yaw, else the row's line65 first segment, else the nearest body (stand-ins included)
        string yawArg = N53RkOpt(args, "--yaw", null);
        List<double[]> line = Tour65.Line(r.row);
        double yaw = 0, pitch = 0;
        string aim = null;
        if (yawArg != null) { yaw = double.Parse(yawArg, V65Ci) * Math.PI / 180; aim = "--yaw " + yawArg; }
        else if (line.Count >= 2 && N53RkOpt(args, "--pose", null) == null)
        {
            Vec3 a = new Vec3(line[0][0], line[0][1] + 1.0, line[0][2]), b = new Vec3(line[1][0], line[1][1] + 1.0, line[1][2]);
            yaw = V65Yaw(a, b); pitch = V65Pitch(a, b); aim = "line65's first segment";
        }
        Enemy track = null; // the nearest-body mode keeps the body in its sights through the charge (it walks); a line or a yaw is held fixed
        if (aim == null)
        {
            track = r.Alive.OrderBy(e => (e.AimPoint - sim.Eye).Length).FirstOrDefault();
            yaw = V65Yaw(sim.Eye, track.AimPoint); pitch = V65Pitch(sim.Eye, track.AimPoint); aim = $"the nearest body, tracked ({track.kind}{(track.standIn29 ? ", a stand-in" : "")} #{track.id} at {V65P(track.position)})";
        }
        var standIns = r.Alive.Where(e => e.standIn29).ToList();
        var aliveBefore = r.Alive.ToList();
        int fired = 0, hits = -1, full = 0;
        int kills = 0, standInKills = 0;
        Action offF = sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { fired++; if (hits < 0) hits = e.hits; });
        Action offFull = sim.events.On<ChargeShot27Event>("charge35Full", e => full++);
        Action offK = sim.events.On<KillEvent>("kill", e => { kills++; if (standIns.Any(s => s.id == e.enemy)) standInKills++; });
        InputFrame Aim() { if (track != null && track.alive) { yaw = V65Yaw(sim.Eye, track.AimPoint); pitch = V65Pitch(sim.Eye, track.AimPoint); } return new InputFrame { yaw = yaw, pitch = pitch }; }
        int held = 0, maxHold = MovementMath.StepsFor(4, sim.step);
        // hold to full (charge35Full), 2 steps more, release; then 30 steps for the shot's hits to land
        for (; held < maxHold && full == 0; held++) r.Step(Aim(), new CombatInput { weapon = -1, fire = new Button { held = true, pressed = held == 0 } });
        for (int k = 0; k < 2; k++, held++) r.Step(Aim(), new CombatInput { weapon = -1, fire = new Button { held = true } });
        for (int k = 0; k < 30; k++) r.Step(Aim(), new CombatInput { weapon = -1 });
        offF(); offFull(); offK();
        Console.WriteLine($"  aim {aim}: yaw {V65F(yaw * 180 / Math.PI)} deg, pitch {V65F(pitch * 180 / Math.PI)} deg at the release; held {held} steps ({V65F(held * sim.step)} s), charge35Full {full}, charged shots {fired}; bodies in the room before {aliveBefore.Count} (stand-ins {standIns.Count})");
        bool green = fired >= 1 && hits >= 2;
        Console.WriteLine($"n65verb pierce: {(green ? "GREEN" : "RED")} | room {r.rd.id} | the charged shot hit {(hits < 0 ? 0 : hits)} bodies, kills {kills}, stand-ins killed {standInKills}/{standIns.Count} | aim {aim} | pose {r.poseFrom}"
            + (fired == 0 ? " | NO charged shot fired" : hits < 2 ? " | one body or none: no pierce" : ""));
        return green ? 0 : 1;
    }

    // ================================================================ parry
    static int V65Parry(string[] args)
    {
        V65Run r = V65Start(args, "parry");
        if (r.refused != null) { Console.WriteLine($"n65verb parry: RED | {r.refused}"); return 1; }
        SurvivalSim sim = r.sim;
        int tries = int.Parse(N53RkOpt(args, "--tries", "1"), V65Ci);
        var look = new InputFrame { yaw = r.yawDeg * Math.PI / 180 };
        if (!V65Wave(r, V65WaveWait, 0.5, look)) { Console.WriteLine($"n65verb parry: RED | no target: no live enemy in {r.rd.id} within {V65WaveWait} s of the pose"); return 1; }
        // the spitters may come in a later wave: an earlier wave with no spitter is killed outright (Loadout.Damage, n53ridekill's way) after
        // 2 s so the next one comes, up to 40 s; the line says how many bodies were killed that way
        double tw = sim.SimTime, quiet = sim.SimTime;
        int forced = 0;
        while (!r.Alive.Any(e => e.kind == "spitter") && sim.SimTime - tw < 40)
        {
            if (sim.SimTime - quiet > 2 && r.Alive.Any(e => !e.standIn29))
            {
                foreach (Enemy e in r.Alive.Where(e => !e.standIn29).ToList()) { sim.loadout.Damage(e, 1e6, false, "revolver", sim.Eye); forced++; }
                quiet = sim.SimTime;
            }
            r.Step(look, new CombatInput { weapon = -1 });
        }
        if (forced > 0) Console.WriteLine($"  earlier waves killed outright to reach a spitter: {forced} bodies");
        Enemy sp = r.Alive.Where(e => e.kind == "spitter").OrderBy(e => V65Flat(e.position, sim.mover.position)).FirstOrDefault();
        if (sp == null) { Console.WriteLine($"n65verb parry: RED | no target: no spitter in {r.rd.id} in any wave within 40 s ({forced} bodies of earlier waves killed outright; alive now: {string.Join(", ", r.Alive.Select(e => e.kind))})"); return 1; }
        SpitterAcid acid = sim.husks.acid;
        double reach = sim.cfg.Num("orb.reflectRange");
        Parry36 pc = sim.parry36Core;
        int reflects0 = acid.reflects, parries0 = pc != null ? pc.parries : 0, bursts0 = pc != null ? pc.bursts : 0, burstHits0 = pc != null ? pc.burstHits : 0, burstKills0 = pc != null ? pc.burstKills : 0;
        int orbHits = 0, punches = 0, parryEv = 0;
        string spKilledBy = null;
        var victims = new HashSet<int>();
        Action offH = sim.events.On<HitEvent>("hit", e => { if (e.weapon == "orb" || e.weapon == "parry36" || e.weapon == "launch36") { orbHits++; victims.Add(e.enemy); } });
        Action offP = sim.events.On(Parry36.Event, o => parryEv++);
        Action offK = sim.events.On<KillEvent>("kill", e => { if (e.enemy == sp.id) spKilledBy = e.weapon; });
        double closestGlob = double.MaxValue;
        int volleys0 = acid.volleys;
        int t = 0, lastPunch = -100, swingPresses = 0;
        for (; t < MovementMath.StepsFor(20, sim.step) && punches < tries && spKilledBy == null; t++)
        {
            Vec3 eye = sim.Eye, at = sp.AimPoint;
            var f = new InputFrame { yaw = V65Yaw(eye, at), pitch = V65Pitch(eye, at) };
            bool press = false;
            foreach (SpitterAcid.Glob g in acid.globs)
            {
                if (!g.alive || g.reflected) continue;
                double d = (g.position - eye).Length;
                closestGlob = Math.Min(closestGlob, d);
                if (d <= reach * 0.9 && t - lastPunch > 12) press = true;
            }
            if (press) { punches++; lastPunch = t; }
            // NIGHT68 g7 (R: the M3 probe RED on r2's melee-only g7, "parried no", the punch dropped with the gauntlet in hand by
            // Rules30.MeleeBlocked): with the glove in hand the press is the swing (fire), which sends a glob back (Combat.SwingGauntlet:
            // Foes40's SwingReflect40 first), as the verb pilot does (Harness.N65.VerbPilot.cs, the parry episode); any other hand: the punch
            bool glove = sim.loadout.Weapon == "gauntlet";
            if (press && glove) swingPresses++;
            r.Step(f, press ? (glove ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }) : new CombatInput { weapon = -1 });
        }
        // the reflected glob's flight and its burst: 3 s more, still facing the spitter
        for (int k = 0; k < MovementMath.StepsFor(3, sim.step); k++)
        {
            Vec3 eye = sim.Eye, at = sp.AimPoint;
            r.Step(new InputFrame { yaw = V65Yaw(eye, at), pitch = V65Pitch(eye, at) }, new CombatInput { weapon = -1 });
        }
        offH(); offP(); offK();
        bool parried = acid.reflects > reflects0;
        int burstHits = pc != null ? pc.burstHits - burstHits0 : 0;
        Console.WriteLine($"  spitter #{sp.id} at {V65P(sp.position)}, {V65F(V65Flat(sp.position, sim.mover.position))} m off; its volleys {acid.volleys - volleys0}, the closest glob {V65F(closestGlob)} m (reach {V65F(reach)} m), punches {punches} (of them the glove's swing {swingPresses}; in hand {sim.loadout.Weapon}), parry36 events {parryEv}, parry36 {(pc != null ? "on" : "OFF")}");
        bool green = parried;
        Console.WriteLine($"n65verb parry: {(green ? "GREEN" : "RED")} | room {r.rd.id} | parried {(parried ? "yes" : "no")} ({acid.reflects - reflects0} glob(s)), the return's hits {orbHits} (bodies {victims.Count}), spitter {(spKilledBy != null ? "killed (" + spKilledBy + ")" : "alive")}"
            + $" | parry36 return bursts {(pc != null ? pc.bursts - bursts0 : 0)} (hits {burstHits}, kills {(pc != null ? pc.burstKills - burstKills0 : 0)}; a second victim {(burstHits >= 2 || victims.Count >= 2 ? "yes" : "no")}) | pose {r.poseFrom}"
            + (punches == 0 ? $" | no glob came inside {V65F(reach * 0.9)} m in 20 s" : ""));
        return green ? 0 : 1;
    }

    // ================================================================ ride (NIGHT66 T1c)
    /// <summary>`n65verb ride [--shoot] [--plant-target auto|x,y,z] [--face k] [--pose ...] [--seed n]`: the ride pilot in the room's own
    /// geometry with the room's wave live (V65Start: the campaign holds the room, invulnerable). Its ride face is one of the room's `ride36`
    /// boxes (the nearest to the pose, or --face k in the file's order); the rider is put 1.2 m up beside the face's near end, moving along it
    /// at 11 m/s with jump held and the stick along and into it (n53ridekill's entry), until the Mover is in WallRide; the ride lasts until it
    /// ends (5 s at most). Without --shoot the look stays along the face (the verb pilot's ride: NIGHT65's pilots never shot from the wall,
    /// n65-data.md:104-105). With --shoot, each step on the wall the look is STEERED OFF THE WALL at a target: the planted one, else the room's
    /// nearest live body in clear sight on the open side, and the stick is re-aimed under that look so the ride holds (the a253 reel's
    /// re-aim); a one-step press every 12 steps while the line is clear. A kill counts FROM THE WALL when the last shot before it was fired in
    /// WallRide within 0.6 s. `--plant-target auto` holds a husk (its own hp) 10 m along the face and 3 m out, at the face's foot; x,y,z puts it
    /// there. GREEN: --shoot, the ride happened and at least one kill from the wall (the planted target's among them when planted); without
    /// --shoot: the ride happened (its kills from the wall printed, 0 expected). Probe numbers, not gameplay.</summary>
    /// <summary>NIGHT67 T1c (the lead: "on level 2 (g8) and kb3 the pilot holds the gauntlet and fires 0 shots, so M3 for a ride room can never
    /// pass"): V65Start puts the player in the room by DOORFREE, so the level's room awards of the rooms before it were never given (iceline's
    /// KESTREL in g6). With --shoot, before the ride: every award (design.n40 row's awards, read from the run's own lists by reflection: harness
    /// code only) whose room comes at or before this room in the level's order and that the run does not own is added, then the pilot switches
    /// to the best owned gun (revolver = the KESTREL, then shotgun, then any owned weapon but the gauntlet). `--weapon <name>` forces that
    /// weapon (`--weapon gauntlet`: the planted RED), `--weapon keep` leaves the hand as the run has it. No gun owned: said, the hand kept.</summary>
    static void V67RideGun(V65Run r, string force, InputFrame look)
    {
        SurvivalSim sim = r.sim;
        var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var ar = typeof(SurvivalSim).GetField("n40AwardRooms", bf)?.GetValue(sim) as List<string>;
        var aw = typeof(SurvivalSim).GetField("n40AwardWeapons", bf)?.GetValue(sim) as List<string>;
        var given = new List<string>();
        string before = string.Join(" ", sim.loadout.owned) + " (in hand " + sim.loadout.Weapon + ")";
        if (force != "keep" && ar != null && aw != null)
            for (int i = 0; i < ar.Count && i < aw.Count; i++)
            {
                int at = r.level.rooms.FindIndex(x => x.id == ar[i]);
                if (at < 0 || at > r.room || sim.loadout.owned.Contains(aw[i])) continue;
                if (sim.loadout.AddWeapon(aw[i])) given.Add(aw[i] + " (" + ar[i] + "'s award)");
            }
        string gun = force == "keep" ? null : force ?? new[] { "revolver", "shotgun" }.FirstOrDefault(w => sim.loadout.owned.Contains(w)) ?? sim.loadout.owned.FirstOrDefault(w => w != "gauntlet");
        string note;
        if (force == "keep") note = "--weapon keep: the hand as the run has it";
        else if (gun == null) note = "the level's loadout holds NO gun at this room (owned: " + string.Join(" ", sim.loadout.owned) + "): ride --shoot cannot fire";
        else if (!sim.loadout.owned.Contains(gun)) note = "--weapon " + gun + ": not owned at this room (owned: " + string.Join(" ", sim.loadout.owned) + "): the hand kept";
        else
        {
            int slot = sim.loadout.owned.IndexOf(gun);
            for (int i = 0; i < 360 && (sim.loadout.Weapon != gun || i < 24); i++) r.Step(look, new CombatInput { weapon = slot });
            note = (force != null ? "--weapon " + gun : "the best owned gun " + gun) + (sim.loadout.Weapon == gun ? "" : " (the switch did NOT take: in hand " + sim.loadout.Weapon + ")");
        }
        Console.WriteLine($"  the ride's gun: owned at the start {before}; awards given up to {r.rd.id}: {(given.Count > 0 ? string.Join(", ", given) : "none")}{(ar == null ? " (the run's award lists not found)" : "")}; {note}; in hand {sim.loadout.Weapon}");
    }

    /// <summary>NIGHT67 T1c (the lead's go): the PUT-IN's loadout. skip v2 (V65ClearRun) and n46flow put the player in the measured room by
    /// Respawn + DOORFREE, so the level's room awards of the rooms BEFORE it were never given (iceline's KESTREL in g6: g7 / g8 played with the
    /// gauntlet alone; kiln's revolver in kb2b). Right after the put-in this gives each such award exactly as the game does on entering its room
    /// (Campaign24.Award31: marked given, the loadout's AddWeapon, which takes it into the hand, the weaponEarned event), skipping a weapon the
    /// run owns or dropped (loom40). The awards come from the run's own design.n40 lists (read by reflection: harness code only). The room's
    /// OWN award is the game's (N40RoomAwardsStep on entry). `--no-put-in-awards` (V67PutInOff) keeps the old put-in (the planted RED).</summary>
    internal static bool V67PutInOff;
    static readonly HashSet<string> V67PutInSaid = new HashSet<string>();
    internal static List<string> V67PutInAwards(SurvivalSim sim, LevelDef level, int roomIndex)
    {
        var given = new List<string>();
        if (V67PutInOff || sim.campaign == null || level == null || roomIndex <= 0) return given;
        var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var ar = typeof(SurvivalSim).GetField("n40AwardRooms", bf)?.GetValue(sim) as List<string>;
        var aw = typeof(SurvivalSim).GetField("n40AwardWeapons", bf)?.GetValue(sim) as List<string>;
        var drops = typeof(SurvivalSim).GetField("n40Drops", bf)?.GetValue(sim) as ICollection<string>;
        if (ar == null || aw == null) return given;
        for (int i = 0; i < ar.Count && i < aw.Count; i++)
        {
            int at = level.rooms.FindIndex(x => x.id == ar[i]);
            string w = aw[i];
            if (at < 0 || at >= roomIndex || (drops != null && drops.Contains(w)) || sim.loadout.owned.Contains(w) || sim.campaign.Given31(w)) continue;
            sim.campaign.Award31(w);
            given.Add(w + " (" + ar[i] + "'s award)");
        }
        string key = level.id + ":" + roomIndex;
        if (V67PutInSaid.Add(key)) Console.WriteLine($"  put-in loadout ({level.rooms[roomIndex].id}, BEFORE entering it: the room's own award, if any, comes on entry): awards of the rooms before it given: {(given.Count > 0 ? string.Join(", ", given) : "none")}; owned {string.Join(" ", sim.loadout.owned)}{(V67PutInOff ? " (--no-put-in-awards: the old put-in)" : "")}");
        return given;
    }

    /// <summary>NIGHT67 T1c (S: "skip v2 enters kb3 with only the earlier rooms' awards"): a PRINT only (no input, no state): the loadout 0.5 s
    /// (60 steps) after the run first holds the measured room: owned and in hand. The game puts an awarded weapon in hand only on a step whose
    /// input asks for no weapon (Combat.cs:722 `if (pendingSlot >= 0 && input.weapon < 0)`), so what is in hand after a room's own award
    /// depends on the instrument's input that step. Once per process per (instrument, level, room).</summary>
    static SurvivalSim v67EntrySim;
    static int v67EntrySteps = -1;
    internal static void V67SayEntry(SurvivalSim sim, LevelDef level, int a, string tag)
    {
        string key = "entry:" + tag + ":" + level.id + ":" + a;
        if (V67PutInSaid.Contains(key)) return;
        if (!ReferenceEquals(sim, v67EntrySim)) { v67EntrySim = sim; v67EntrySteps = -1; }
        if (sim.campaign == null || sim.campaign.room != a) return;
        if (++v67EntrySteps < 60) return;
        V67PutInSaid.Add(key);
        Console.WriteLine($"  entry loadout ({tag}, {level.rooms[a].id}, 0.5 s after entering): owned {string.Join(" ", sim.loadout.owned)}, in hand {sim.loadout.Weapon}");
    }
    static string V67Pending(SurvivalSim sim)
    {
        var f = sim.loadout.GetType().GetField("pendingSlot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int p = f != null ? (int)f.GetValue(sim.loadout) : -1;
        return p >= 0 && p < sim.loadout.owned.Count ? sim.loadout.owned[p] + " (pending: goes in hand on the next step with no weapon input)" : "none";
    }

    static int V66Ride(string[] args)
    {
        V65Run r = V65Start(args, "ride");
        if (r.refused != null) { Console.WriteLine($"n65verb ride: RED | {r.refused}"); return 1; }
        SurvivalSim sim = r.sim;
        bool shoot = Array.IndexOf(args, "--shoot") >= 0;
        string plantArg = N53RkOpt(args, "--plant-target", null);
        var look0 = new InputFrame { yaw = r.yawDeg * Math.PI / 180 };
        if (shoot) V67RideGun(r, N53RkOpt(args, "--weapon", null), look0);
        bool wave = V65Wave(r, V65WaveWait, 1.0, look0);
        // the room's ride faces (V65VerbPilot's parse): [axis 0 x-plane / 1 z-plane, plane, normal sign, lo, hi, top, bottom]
        var faces = new List<double[]>();
        foreach (object o in PortData.Arr(PortData.Obj(r.json["campaign"])["boxes"]))
        {
            var b = PortData.Obj(o);
            if (!(b.TryGetValue("ride", out object rv) && rv is bool rb && rb) || !b.TryGetValue("ride36", out object r36)) continue;
            if (!(b.TryGetValue("zone", out object zo) && zo as string == r.rd.id)) continue;
            string face = PortData.Obj(r36).TryGetValue("face", out object fo) ? (string)fo : "E";
            var mn = PortData.Arr(b["min"]); var mx = PortData.Arr(b["max"]);
            double x0 = PortData.Num(mn[0]), y0 = PortData.Num(mn[1]), z0 = PortData.Num(mn[2]), x1 = PortData.Num(mx[0]), top = PortData.Num(mx[1]), z1 = PortData.Num(mx[2]);
            faces.Add(face == "E" ? new[] { 0, x1, 1, z0, z1, top, y0 } : face == "W" ? new[] { 0, x0, -1, z0, z1, top, y0 } : face == "S" ? new[] { 1, z1, 1, x0, x1, top, y0 } : new[] { 1, z0, -1, x0, x1, top, y0 });
        }
        if (faces.Count == 0) { Console.WriteLine($"n65verb ride: RED | room {r.rd.id} has no ride36 face in {r.level.id}"); return 1; }
        Vec3 pose = r.pose;
        double FaceDist(double[] w) { double s = w[0] == 0 ? pose.z : pose.x, sc = Math.Max(w[3], Math.Min(w[4], s)), off = (w[0] == 0 ? pose.x : pose.z) - w[1]; return Math.Sqrt(off * off + (s - sc) * (s - sc)); }
        string faceArg = N53RkOpt(args, "--face", null);
        double[] fw = faceArg != null ? faces[int.Parse(faceArg, V65Ci)] : faces.OrderBy(FaceDist).First();
        double fsPose = fw[0] == 0 ? pose.z : pose.x;
        bool toHi = (fw[4] - fsPose) >= (fsPose - fw[3]); // ride toward the far end
        Vec3 n = fw[0] == 0 ? new Vec3(fw[2], 0, 0) : new Vec3(0, 0, fw[2]);
        Vec3 u = fw[0] == 0 ? new Vec3(0, 0, toHi ? 1 : -1) : new Vec3(toHi ? 1 : -1, 0, 0);
        double sStart = toHi ? fw[3] + 1 : fw[4] - 1, faceLen = fw[4] - fw[3];
        const double off = 0.45, ahead = 10, out3 = 3; // n53ridekill's stand-off; the auto target 10 m along, 3 m out
        Vec3 f0 = fw[0] == 0 ? new Vec3(fw[1] + fw[2] * off, fw[6], sStart) : new Vec3(sStart, fw[6], fw[1] + fw[2] * off);
        Console.WriteLine($"  face {faces.IndexOf(fw)} of {faces.Count}: {(fw[0] == 0 ? "x" : "z")} = {V65F(fw[1])}, {V65F(fw[3])}..{V65F(fw[4])} ({V65F(faceLen)} m), top {V65F(fw[5])}, normal ({V65F(n.x)}, {V65F(n.z)}), ride along ({V65F(u.x)}, {V65F(u.z)}) from {V65P(f0)}; the room's wave {(wave ? "live" : "NOT live")}, alive {r.Alive.Count()}; weapon {sim.loadout.Weapon}");
        // the planted target: a husk held in place (its own hp), at the face's foot 10 m along and 3 m out (auto) or at x,y,z
        Husk planted = null; Vec3 spot = default;
        if (plantArg != null)
        {
            if (plantArg == "auto") spot = new Vec3(f0.x + u.x * ahead + n.x * (out3 - off), fw[6], f0.z + u.z * ahead + n.z * (out3 - off));
            else { double[] v = plantArg.Split(',').Select(q => double.Parse(q, V65Ci)).ToArray(); spot = new Vec3(v[0], v[1], v[2]); }
            planted = sim.husks.Spawn("husk", spot, f0);
            if (planted == null) { Console.WriteLine($"n65verb ride: RED | the planted husk did not spawn at {V65P(spot)}"); return 1; }
            for (int i = 0; i < 120; i++) { planted.position = planted.previous = spot; r.Step(new InputFrame { yaw = look0.yaw }, new CombatInput { weapon = -1 }); } // its spawn tell waited out (n53ridekill)
            Console.WriteLine($"  planted target: husk #{planted.id} held at {V65P(spot)}, hp {V65F(planted.hp)}");
        }
        // the pool reuses a dead body's object (and its id) for the next spawn: the plant is its first id, held only until that body dies
        int plantedId = planted != null ? planted.id : -1;
        bool plantedDead = false;
        bool PlantLive() => planted != null && !plantedDead && planted.alive && planted.id == plantedId;
        // kills from the wall: the last shot fired in WallRide within 0.6 s before the kill
        int stp = 0, lastFire = -1000, shotsWall = 0, shotsOff = 0, killsWall = 0, killsOther = 0;
        bool lastFireWall = false, plantedWall = false;
        var killLines = new List<string>();
        int window = MovementMath.StepsFor(0.6, sim.step);
        Action offF = sim.events.On("fire", o => { lastFire = stp; lastFireWall = sim.mover.state == MoveState.WallRide; if (lastFireWall) shotsWall++; else shotsOff++; });
        Action offK = sim.events.On<KillEvent>("kill", e =>
        {
            bool wall = lastFireWall && stp - lastFire <= window && e.weapon != "launch36";
            if (wall) killsWall++; else killsOther++;
            bool isPlant = !plantedDead && e.enemy == plantedId;
            if (isPlant) { plantedDead = true; if (wall) plantedWall = true; }
            killLines.Add($"    kill {e.kind} #{e.enemy} by {e.weapon} at {V65P(e.position)}, {V65F(e.distance)} m, step {stp}: {(wall ? "FROM THE WALL" : "not from the wall")}{(isPlant ? " (the planted target)" : "")}");
        });
        // the entry: 1.2 m up beside the face, along it at 11 m/s, a push into it, jump held, the stick along and into it
        RideKill53Wall.Stick(u, n, 0.5, out double ryaw, out double rmx, out double rmz); // n: the face's outward normal (Stick's convention)
        sim.Respawn(new Vec3(f0.x, f0.y + 1.2, f0.z), ryaw * 180 / Math.PI);
        sim.mover.velocity = new Vec3(u.x * 11 - n.x * 1.0, 0, u.z * 11 - n.z * 1.0);
        int rideStarts = 0, rideSteps = 0, t = 0;
        Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.WallRideStart) rideStarts++; };
        sim.mover.Events += onMove;
        bool rode = false, ended = false;
        double maxOff = 0, rideFrom = double.NaN, rideTo = double.NaN;
        int lastPress = -100, after = 0;
        Enemy tgt = null;
        int maxT = MovementMath.StepsFor(5, sim.step);
        for (; t < maxT && after < MovementMath.StepsFor(0.6, sim.step); t++, stp++)
        {
            bool riding = sim.mover.state == MoveState.WallRide;
            if (riding) { rideSteps++; if (!rode) rideFrom = (sim.mover.position.x * u.x + sim.mover.position.z * u.z); rode = true; rideTo = sim.mover.position.x * u.x + sim.mover.position.z * u.z; }
            if (rode && !riding) ended = true;
            if (ended) after++;
            if (!rode && t > 90) break; // never got on the wall
            if (PlantLive()) { planted.position = planted.previous = spot; }
            var f = new InputFrame { yaw = ryaw, moveX = rmx, moveZ = rmz, jump = new Button { held = !ended } };
            var c = new CombatInput { weapon = -1 };
            if (shoot && riding)
            {
                Vec3 eye = sim.Eye;
                tgt = PlantLive() ? planted : r.Alive.Where(e => e.alive && (e.AimPoint - eye).x * n.x + (e.AimPoint - eye).z * n.z > -0.5 && (e.AimPoint - eye).Length < 40 && RideKill53Wall.LineClear(sim.world, eye, e.AimPoint)).OrderBy(e => (e.AimPoint - eye).Length).FirstOrDefault();
                if (tgt != null)
                {
                    // the a253 reel's re-aim: the look at the target, the stick under that yaw kept along the wall and a little into it
                    Vec3 at = tgt.AimPoint;
                    double dx = at.x - eye.x, dy = at.y - eye.y, dz = at.z - eye.z;
                    double ay = Math.Atan2(-dx, -dz), ap = Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
                    double wx = u.x - n.x * 0.5, wz = u.z - n.z * 0.5, wl = Math.Sqrt(wx * wx + wz * wz);
                    double fx = -Math.Sin(ay), fz = -Math.Cos(ay), rx = Math.Cos(ay), rz = -Math.Sin(ay);
                    f = new InputFrame { yaw = ay, pitch = ap, moveZ = (wx * fx + wz * fz) / wl, moveX = (wx * rx + wz * rz) / wl, jump = new Button { held = true } };
                    double faceYaw = Math.Atan2(-u.x, -u.z), d = Math.Abs(((ay - faceYaw) % (2 * Math.PI) + 3 * Math.PI) % (2 * Math.PI) - Math.PI);
                    maxOff = Math.Max(maxOff, d * 180 / Math.PI);
                    if (t - lastPress >= 12 && RideKill53Wall.LineClear(sim.world, eye, at)) { lastPress = t; c.fire = new Button { held = true, pressed = true }; }
                }
            }
            r.Step(f, c);
        }
        sim.mover.Events -= onMove;
        offF(); offK();
        foreach (string l in killLines) Console.WriteLine(l);
        double rideLen = double.IsNaN(rideFrom) ? 0 : Math.Abs(rideTo - rideFrom);
        bool green = rode && (!shoot || killsWall >= 1) && (planted == null || !shoot || plantedWall);
        Console.WriteLine($"n65verb ride{(shoot ? " --shoot" : "")}: {(green ? "GREEN" : "RED")} | room {r.rd.id} | ride {(rode ? $"yes ({V65F(rideSteps * sim.step)} s on the wall, {V65F(rideLen)} m along it, WallRideStart {rideStarts})" : "NO")}"
            + $" | look off the wall at most {V65F(maxOff)} deg | shots from the wall {shotsWall} (off it {shotsOff}) | kills from the wall {killsWall} (other kills {killsOther})"
            + (planted != null ? $" | the planted target #{plantedId} {(!plantedDead ? "ALIVE" : plantedWall ? "killed from the wall" : "killed, not from the wall")}" : "") + $" | pose {r.poseFrom}"
            + (!rode ? " | the rider never got on the face" : shoot && killsWall == 0 ? " | no kill from the wall" : ""));
        return green ? 0 : 1;
    }

    // ================================================================ skip
    /// <summary>The walk pilot with one verb's input refused (or none: the control).</summary>
    sealed class V65Filter : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly IPlayerInput inner;
        readonly string refuse;
        readonly double reach;
        public int refusedPresses;
        bool fireHeldLast;
        public V65Filter(SurvivalSim sim, IPlayerInput inner, string refuse) { this.sim = sim; this.inner = inner; this.refuse = refuse; reach = sim.cfg.Num("orb.reflectRange"); }
        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public InputFrame SampleMove(bool active)
        {
            InputFrame f = inner.SampleMove(active);
            // NIGHT66 T1b fix 2 (Logs/n66/verdict-T1b.md problem 1, verdict-T1b-r2.md): the ride refused, nothing else. NIGHT65's rule dropped
            // every held jump in the air; round 1's fix cut the stick at any face in reach (round 2's critic: no minWallHeight test, and it cut the
            // stick's length while ride40 tests its direction, so it cost the refused runs air control at walls never rideable). Now only the
            // backstop: the input is the walker's own until a ride has started; on a step in WallRide the stick points away from the wall, so the
            // Mover's away rule ends the ride (one step on the wall per started ride, counted). The jump and the dash are never touched.
            if (refuse == "ride") { NoRide68(); RefuseRide(ref f); }
            if (refuse == "slide" && (f.slide.held || f.slide.pressed)) { f.slide = default; refusedPresses++; }
            return f;
        }

        /// <summary>NIGHT68 I1 (the slate's I1 line 2: the refused ride run rides 0 times): the ride refused BEFORE it starts. The measured room
        /// (room68; with none, the backstop below alone, as NIGHT66-67) is given the game's own noride rule (Mover.SetNoRide38: no ride starts on,
        /// is drawn onto or turns onto a wall inside a noride box; Ride38.cs), the level's own boxes kept; every other input is the walker's.
        /// Each wall contact in the air the rule blocked (a run of steps whose Mover counted a noride block) is one refusal (`refused ride N`,
        /// rideContacts). The backstop (RefuseRide) stays as the leak's detector: a ride that still starts is ended and counted in ridesCaught.</summary>
        void NoRide68()
        {
            if (room68 == null) return;
            Ride38.Box[] cur = sim.mover.flow34.ride35.noRide;
            if (set68 == null || cur != set68) { set68 = V68NoRide(cur, room68, null, sim.cfg.movement.wallRide.probeDistance); sim.mover.SetNoRide38(set68); }
            int blocks = sim.mover.flow34.ride35.r38NoRideBlocks;
            bool blocked = blocks68 >= 0 && blocks > blocks68;
            if (blocked && !blockedLast68) { rideContacts++; refusedPresses++; }
            blockedLast68 = blocked; blocks68 = blocks;
        }
        public RoomDef room68; // NIGHT68 I1: the measured room (skip v2), given noride in a refused ride run
        Ride38.Box[] set68;
        int blocks68 = -1; bool blockedLast68;
        public int rideContacts;

        /// <summary>NIGHT66 T1b fix 2: the backstop only. On a step the Mover is in WallRide (the ride has started), the stick points away from
        /// the wall (its normal, outward), past wallRide.awayDot, so StepWallRide's away rule ends the ride (EndRide("away")); a new ride counts
        /// once (ridesCaught), each step on the wall too (rideEnded). Every other step the walker's input passes untouched.</summary>
        void RefuseRide(ref InputFrame f)
        {
            Mover m = sim.mover;
            bool riding = m.state == MoveState.WallRide;
            if (riding)
            {
                if (!wasRiding) ridesCaught++;
                double ax = m.wallNormal.x, az = m.wallNormal.z, al = Math.Sqrt(ax * ax + az * az);
                if (al > 1e-6)
                {
                    double gx = -Math.Sin(f.yaw), gz = -Math.Cos(f.yaw), hx = Math.Cos(f.yaw), hz = -Math.Sin(f.yaw);
                    f.moveZ = (ax * gx + az * gz) / al; f.moveX = (ax * hx + az * hz) / al;
                    rideEnded++; refusedPresses++;
                }
            }
            wasRiding = riding;
        }
        bool wasRiding;
        public int rideSteered, rideDashes, rideEnded, ridesCaught; // rideSteered / rideDashes: round 1's cut, gone (always 0)
        public int gauntletRefused; // NIGHT67 T1b: the gauntlet's swing / LT presses dropped near a glob (refuse parry)
        public long lastRefusedStep68 = -1, lastPassStep68 = -1; public string lastPassKind68; // NIGHT68 g7 leak trace (print only)
        // NIGHT67 T1b round 2 (g7: 12 / 7 / 7 globs parried with the 1.5 x reflectRange = 3 m radius): design.n43.parry43 is ON and its catch
        // reaches a glob along its path with a buffered press (its freeze: "the earliest parry 1.94 -> 5.61 m"), so a press refused only within
        // 3 m leaked; the refusal radius is V65ParryRefuseM (8 m: 5.61 m plus a buffered press at a 16 m/s glob), for the punch and the gauntlet
        const double V65ParryRefuseM = 8.0; // m, a probe number (the harness's refusal), not gameplay
        bool GlobInReach() { foreach (SpitterAcid.Glob g in sim.husks.acid.globs) if (g.alive && !g.reflected && (g.position - sim.Eye).Length <= Math.Max(reach * 1.5, V65ParryRefuseM)) return true; return false; }
        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = inner.SampleCombat(active);
            // taps only, no charge: fire is never held on two steps in a row (the autopilot presses on every step it fires, so a continuous
            // hold reads as held + pressed each step; the loadout would charge it)
            // NIGHT66 T1b fix (the critic's problem 6): only the KESTREL's charge (the pierce) is refused; every other gun fires as it did
            if (refuse == "pierce") { if (sim.loadout.Weapon == "revolver" && c.fire.held && fireHeldLast) { c.fire = default; refusedPresses++; } fireHeldLast = c.fire.held; }
            if (c.punch.pressed || c.punch.held)
            {
                bool speed = sim.mover.state == MoveState.Dash || sim.mover.state == MoveState.Slide || sim.mover.state == MoveState.WallRide || (sim.launch36Core != null && sim.launch36Core.graceLeft > 0);
                bool glob = refuse == "parry" && GlobInReach();
                if ((refuse == "launch" && speed) || (refuse == "parry" && glob)) { c.punch = default; refusedPresses++; }
            }
            // NIGHT67 T1b (Logs/n67/verdict-cards.md item B: g7's refused run parried 12 / 12 / 12 globs): with the GAUNTLET in hand the punch is
            // dropped by the game (Rules30.MeleeBlocked) and a glob goes back by the gauntlet's SWING on fire (Combat Foes40.cs SwingReflect40) and
            // by its LT parry window on altFire (LtAlways31.cs StepLt31): the parry refusal drops those presses too, by the punch's own rule
            // (a live glob, not yet reflected, within 1.5 x orb.reflectRange of the eye). The gauntlet's swing near a glob is refused for melee
            // too, as the punch near a glob always was. No other weapon reflects on fire / altFire (StepLt31 zeroes the window off the gauntlet).
            // NIGHT68 g7 (the lead: "narrow the parry refusal to the parry, not every swing near a glob"; R's g7 is melee only, so the 8 m rule
            // dropped the refused run's every swing near a spitter): a gauntlet press is refused only when it would send a glob back, by the
            // swing or by the orb buffer it opens (V68GlobCatchable: the game's own catch tests, Harness.N68.Glob.cs); every other swing passes
            if (refuse == "parry" && sim.loadout.Weapon == "gauntlet" && (c.fire.pressed || c.fire.held || c.altFire.pressed || c.altFire.held) && V68GlobCatchable(sim, Yaw, Pitch, out _))
            {
                c.fire = default; c.altFire = default; refusedPresses++; gauntletRefused++;
                lastRefusedStep68 = (long)Math.Round(sim.SimTime / sim.step);
            }
            // NIGHT68 g7 leak trace (print only): the last gauntlet press that passed the filter (fire = the swing, altFire = LT)
            if (refuse == "parry" && sim.loadout.Weapon == "gauntlet" && (c.fire.pressed || c.altFire.pressed)) { lastPassStep68 = (long)Math.Round(sim.SimTime / sim.step); lastPassKind68 = c.altFire.pressed ? "altFire (LT)" : "fire (the swing)"; }
            return c;
        }
    }

    sealed class V65Clear
    {
        public bool cleared; public double clearS = double.NaN; public int deaths, falls, launches, charged, parries, rides, slides, refused, top; public long fight, aboveDull, trigger, verbRefused;
        // NIGHT66 T1b (skip v2): the run moved and fired (metres walked in the room's fight, shots fired there), the declined episodes and the
        // inner pilot's dropped presses (refused = their sum), the steps the player touched each piece of the room (V66Pieces' keys), and
        // launch36's bursts that killed, with where
        public double walked; public int shots, declined, innerRefused;
        /// <summary>NIGHT66 T1b: the spitters' globs reflected in the room's fight (SpitterAcid.reflects): skip v2's parry uses. The planted
        /// no-fire room showed Parry36.Event firing on the autopilot's melee parries in every run (5 / 5 / 5 with no spitter in the room).</summary>
        public int reflects;
        /// <summary>NIGHT66 T1b fix: the ride refusal's own counts (V65Filter.RefuseRide): steps whose stick was taken off a wall in reach, dash
        /// presses into a wall dropped.</summary>
        public int rideSteered, rideDashes, rideEnded, rideSteps, ridesCaught;
        public Dictionary<string, int> touches = new Dictionary<string, int>();
        public List<string> launchKills = new List<string>();
        public List<string> launchEnds = new List<string>(); // NIGHT70 Z2: the throw chart (the plain verb-aware run): each launch's start, end and outcome
        public string plantNote;
        // builder R's asks (via the lead): the bodies each charged shot hit (ChargeShot27Event.hits), the first wall ride's time from the room's
        // entry and where it started
        public List<int> chargedHits = new List<int>();
        public double firstRideS = double.NaN; public Vec3 firstRideAt;
        public string launchLine; // NIGHT68 I2: a launch room's verb-aware run: its targets, punches and kills (V68LaunchLine)
        public Dictionary<string, int> rideFaces = new Dictionary<string, int>(); // NIGHT68 I1: the room's wall rides by the face they started on
        public int rideContacts, offlineCut; public string lineFace, refusedWhy;
        public int gauntletRefused; public string handAtEnd; // NIGHT68 g7: the filter's gauntlet presses refused (refuse parry), the weapon in hand at the run's end
        public int swingParries; // NIGHT68 g7: the verb-aware pilot's parry presses made with the glove's swing // NIGHT68 I1: the refusal's blocked wall contacts; the verb-aware run's off-line cuts, face, refusal
        public long commitHeld, commitClose, commitKept, commitSwitch; public int commitBad; // NIGHT70 Z3: the committed run's held-fire steps, close steps, shots in a held step
    }

    // ---- NIGHT70 Z2 (slate Z2; scout.md A "smallest add"): the throw chart of skip v2's plain verb-aware run, print only. After each sim.Step
    // the run's launch36 flights are diffed: a body that entered them is a launch (from = the flight's own start, Launch36.Flight.from); a body
    // that left them is the flight's end: launch36's lastEnd (wall / body / timeout / landed / dead: per step, so two flights ending in one step
    // share it: said "(shared step)"), lastThrown (the flat metres from the start to the flight's last check; a shared step: from -> where the
    // body is), the kills of the step's burst nearest the body (a wall / body end), and then the body is followed up to 3 s from its launch, as
    // the probe follows it (Verb.cs V65Launch): in a pit / in lava = its position inside one of the room's pit rects below y -0.2 (V65Pits'
    // rule). Only launches made while the player is in the room are charted (as launch36's count). Nothing here feeds the run.
    sealed class V70Throw { public Husk body; public int id, start, until; public Vec3 from, to; public string end; public double thrown; public int kills; public bool pit, lava, ended; }

    /// <summary>NIGHT70 Z2: the room's pit rects [x0, z0, x1, z1, lava 0/1] (V65Pits' rule, from the level json).</summary>
    static List<double[]> V70Pits(Dictionary<string, object> json, string roomId)
    {
        var res = new List<double[]>();
        var camp = PortData.Obj(json["campaign"]);
        if (camp.TryGetValue("pits", out object po))
            foreach (object o in PortData.Arr(po))
            {
                var d = PortData.Obj(o);
                if (d.TryGetValue("zone", out object z) && (string)z != roomId) continue;
                var rc = PortData.Arr(d["rect"]);
                bool lava = d.TryGetValue("lava", out object lv) && lv is bool b && b;
                res.Add(new[] { PortData.Num(rc[0]), PortData.Num(rc[1]), PortData.Num(rc[2]), PortData.Num(rc[3]), lava ? 1 : 0 });
            }
        return res;
    }

    static void V70PitCheck(V70Throw t, Vec3 at, List<double[]> pits)
    {
        foreach (double[] p in pits)
            if (at.x > p[0] && at.x < p[2] && at.z > p[1] && at.z < p[3] && at.y < -0.2) { if (p[4] > 0) t.lava = true; else t.pit = true; }
    }

    static string V70Line(V70Throw t) => $"from ({V65F(t.from.x)}, {V65F(t.from.z)}) to {V65P(t.to)} thrown {V65F(t.thrown)} m | end {t.end} | kills {t.kills} | in pit {(t.pit ? "yes" : "no")} | in lava {(t.lava ? "yes" : "no")}";

    static void V70ChartStep(SurvivalSim sim, List<V70Throw> fly, List<V70Throw> post, List<Launch36BurstEvent> bursts, List<double[]> pits, V65Clear res, int stp, bool inRoom)
    {
        Launch36 lc = sim.launch36Core;
        if (lc == null) return;
        // the flights that ended this step (their body left launch36's list, or the body was reused by the pool)
        var ended = fly.Where(t => !lc.flights.Any(f => f.body == t.body && t.body != null && t.body.id == t.id)).ToList();
        foreach (V70Throw t in ended)
        {
            fly.Remove(t);
            bool alive = t.body.alive && t.body.id == t.id;
            t.to = t.body.position;
            t.end = (lc.lastEnd ?? "-") + (ended.Count > 1 ? " (shared step)" : "");
            t.thrown = ended.Count == 1 ? lc.lastThrown : V65Flat(t.from, t.to);
            if (lc.lastEnd == "body" || lc.lastEnd == "wall")
            {
                Launch36BurstEvent nb = bursts.OrderBy(b => (b.position - t.to).Length).FirstOrDefault();
                if (nb != null) t.kills = nb.kills;
            }
            V70PitCheck(t, t.to, pits);
            t.ended = true;
            if (alive && stp < t.until) post.Add(t); else res.launchEnds.Add(V70Line(t));
        }
        // the bodies followed after their flight (up to 3 s from the launch): a pit / the lava; done at the body's end or the 3 s
        for (int i = post.Count - 1; i >= 0; i--)
        {
            V70Throw t = post[i];
            bool alive = t.body.alive && t.body.id == t.id;
            if (alive) V70PitCheck(t, t.body.position, pits);
            if (!alive || stp >= t.until) { post.RemoveAt(i); res.launchEnds.Add(V70Line(t)); }
        }
    }

    /// <summary>NIGHT70 Z2: a LaunchEvent (payload: the body's id): its flight (just made or re-made by Launch36.Launch) starts a charted throw; a
    /// body launched again while still charted in flight ends that throw as "relaunched".</summary>
    static void V70Launched(SurvivalSim sim, List<V70Throw> fly, object o, int stp, V65Clear res)
    {
        Launch36 lc = sim.launch36Core;
        if (lc == null || !(o is int id)) return;
        Launch36.Flight f = lc.flights.FirstOrDefault(x => x.body != null && x.body.id == id);
        if (f == null) return;
        V70Throw old = fly.FirstOrDefault(t => t.body == f.body && t.id == id);
        if (old != null) { fly.Remove(old); old.to = f.body.position; old.end = "relaunched"; old.thrown = V65Flat(old.from, old.to); old.ended = true; res.launchEnds.Add(V70Line(old)); }
        fly.Add(new V70Throw { body = f.body, id = id, from = f.from, start = stp, until = stp + MovementMath.StepsFor(3, sim.step) });
    }

    static void V70ChartEnd(SurvivalSim sim, List<V70Throw> fly, List<V70Throw> post, V65Clear res)
    {
        foreach (V70Throw t in fly) { t.to = t.body.position; t.end = "running at the run's end"; t.thrown = V65Flat(t.from, t.to); res.launchEnds.Add(V70Line(t)); }
        foreach (V70Throw t in post) res.launchEnds.Add(V70Line(t));
        fly.Clear(); post.Clear();
    }

    // NIGHT66 T1b: the planted rooms (skip v2's proof only; `--plant-forced` / `--plant-null`, launch rooms): forced = the room's first
    // launchable husk (re-marked after a room retry) takes damage only from launch36 (the verb); null = launch36's damage x0 (the verb fires and
    // does nothing). Applied through the loadout's bonus hook in V65ClearRun, after StartRun. Probe switches, never gameplay.
    static bool V66PlantForced, V66PlantNull;

    /// <summary>NIGHT66 T1b: a room's pieces for the touch count: its campaign boxes (zone = the room, the floor aside), the collision wedges
    /// (the plan's ramps: x, z = the ramp's `from`) and the pads whose (x, z) lie in the room's rect. Key (2 decimals): `box(min..max)`,
    /// `ramp(x,z,length,width,rise)`, `pad(x,y,z)`, as Tools/night/n66-deco.cjs builds them from a room file. Each: the key, a label (the
    /// box's job and the head of its why), and the geometry [kind 0 box: min, max | 1 wedge: x, z, yaw, length, width, rise | 2 pad: x, y, z].</summary>
    sealed class V66Piece { public string key, label; public int kind; public double[] g; }

    static string V66R(double v) => Math.Round(v, 2).ToString("0.##", V65Ci) == "-0" ? "0" : Math.Round(v, 2).ToString("0.##", V65Ci);

    static List<V66Piece> V66Pieces(Dictionary<string, object> json, RoomDef rd)
    {
        var res = new List<V66Piece>();
        var camp = PortData.Obj(json["campaign"]);
        if (camp.TryGetValue("boxes", out object bo))
            foreach (object o in PortData.Arr(bo))
            {
                var b = PortData.Obj(o);
                if (!(b.TryGetValue("zone", out object z) && z as string == rd.id)) continue;
                if (b.TryGetValue("kind", out object k) && k as string == "floor") continue;
                var mn = PortData.Arr(b["min"]); var mx = PortData.Arr(b["max"]);
                double[] g = { PortData.Num(mn[0]), PortData.Num(mn[1]), PortData.Num(mn[2]), PortData.Num(mx[0]), PortData.Num(mx[1]), PortData.Num(mx[2]) };
                string why = b.TryGetValue("why", out object wo) && wo is string ws ? ws : "";
                int cut = why.IndexOfAny(new[] { ':', '(' });
                string head = (cut > 0 ? why.Substring(0, cut) : why).Trim();
                if (head.Length > 48) head = head.Substring(0, 48);
                string job = b.TryGetValue("job", out object jo) && jo is string js ? js : "-";
                res.Add(new V66Piece { kind = 0, g = g, key = $"box({V66R(g[0])},{V66R(g[1])},{V66R(g[2])}..{V66R(g[3])},{V66R(g[4])},{V66R(g[5])})", label = $"{job}: {head}".Replace("\"", "'") });
            }
        var col = PortData.Obj(json["collision"]);
        if (col.TryGetValue("wedges", out object wo2))
            foreach (object o in PortData.Arr(wo2))
            {
                WedgeSpec w = PortData.Wedge(o);
                if (w.x < rd.x0 || w.x > rd.x1 || w.z < rd.z0 || w.z > rd.z1) continue;
                res.Add(new V66Piece { kind = 1, g = new[] { w.x, w.z, w.yaw, w.length, w.width, w.rise }, key = $"ramp({V66R(w.x)},{V66R(w.z)},{V66R(w.length)},{V66R(w.width)},{V66R(w.rise)})", label = "ramp" });
            }
        if (json.TryGetValue("pads", out object po))
            foreach (object o in PortData.Arr(po))
            {
                var d = PortData.Obj(o);
                double x = PortData.Num(d["x"]), y = PortData.Num(d["y"]), zz = PortData.Num(d["z"]);
                if (x < rd.x0 || x > rd.x1 || zz < rd.z0 || zz > rd.z1) continue;
                res.Add(new V66Piece { kind = 2, g = new[] { x, y, zz }, key = $"pad({V66R(x)},{V66R(y)},{V66R(zz)})", label = "pad" });
            }
        return res;
    }

    /// <summary>NIGHT66 T1b: whether the player's body touches the piece this step: the capsule's box (body.capsuleRadius, capsuleHeight)
    /// grown by 0.15 m overlaps a box (stood on, ridden, leant on); the feet within 0.3 m of a wedge's sloped top over its length; the feet on a
    /// pad's disc (jumpPad.radius + 0.2 m, triggerHeight + 0.2 m). Probe margins, not gameplay.</summary>
    static bool V66Touch(V66Piece pc, Vec3 p, double r, double h, double padR, double padH)
    {
        const double e = 0.15;
        double[] g = pc.g;
        if (pc.kind == 0)
            return p.x + r + e > g[0] && p.x - r - e < g[3] && p.z + r + e > g[2] && p.z - r - e < g[5] && p.y + h + e > g[1] && p.y - e < g[4];
        if (pc.kind == 1)
        {
            double ax = -Math.Sin(g[2]), az = -Math.Cos(g[2]), rx = Math.Cos(g[2]), rz = -Math.Sin(g[2]);
            double dx = p.x - g[0], dz = p.z - g[1], along = dx * ax + dz * az, across = dx * rx + dz * rz;
            if (g[3] < 1e-9 || along < 0.3 || along > g[3] + r || Math.Abs(across) > g[4] / 2 + r) return false;
            double top = g[5] * Math.Min(1, along / g[3]);
            return p.y > top - 0.3 && p.y < top + 0.3;
        }
        double fx = p.x - g[0], fz = p.z - g[2];
        return fx * fx + fz * fz <= (padR + 0.2) * (padR + 0.2) && Math.Abs(p.y - g[1]) <= padH + 0.2;
    }

    /// <summary>NIGHT66 T1b fix 2: `--set <config.path>=<number>` (repeatable; n46flow's --set): a number changed in this process's config only,
    /// for every run of the skip (a planted room by the game's own rules, e.g. design.n40.ride40.minWallHeight=99: no face is tall enough to
    /// ride). A probe: never written to disk.</summary>
    static readonly List<string> V66Sets = new List<string>();
    static void V66ParseSets(string[] args) { V66Sets.Clear(); for (int k = 0; k + 1 < args.Length; k++) if (args[k] == "--set") V66Sets.Add(args[k + 1]); }

    static V65Clear V65ClearRun(string path, string roomId, uint seed, string refuse, double capS, bool verbAware = false, bool refuseInside = false, bool commit = false)
    {
        GameConfig cfg = Config();
        foreach (string kv in V66Sets)
        {
            int eq = kv.IndexOf('=');
            string[] parts = kv.Substring(0, eq).Split('.');
            var o = cfg.root;
            for (int k = 0; k < parts.Length - 1; k++) o = PortData.Obj(o[parts[k]]);
            o[parts[parts.Length - 1]] = double.Parse(kv.Substring(eq + 1), V65Ci);
        }
        var json = V65Planted(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path)))); // NIGHT65 T: --plant-verb65's rows (none: the file)
        SurvivalMap map = SurvivalMap.FromExport(json);
        LevelDef level = LevelDef.From(json);
        int a = level.rooms.FindIndex(x => x.id == roomId);
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
            if (c == null || c.room < 0) return lv.rooms[a].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        var f1 = new F1Input(sim, new Walk38Pilot(sim, ap), json);
        // the runs: verb-aware (vp alone); NIGHT66 T1b the refused-inside run (vp in its refused mode, inside the press filter); the walker /
        // v1's refused run (the filter round the plain pilot; refuse "none": v1's control)
        V65VerbPilot vp = verbAware ? new V65VerbPilot(sim, f1, json) { refuse = refuseInside, commit = commit && !refuseInside } : null; // NIGHT70 Z3: commit = the committed run
        var filter = new V65Filter(sim, vp != null && refuseInside ? (IPlayerInput)vp : f1, refuse) { room68 = level.rooms[a] }; // NIGHT68 I1: the refused ride run's room under noride
        V65Stats.Clear();
        sim.UseInput(vp != null && !refuseInside ? (IPlayerInput)vp : filter);
        var res = new V65Clear();
        int stp = 0, enterStep = -1;
        bool InRoom() => sim.campaign != null && sim.campaign.room == a;
        sim.events.On<RoomEvent>("roomEnter", e => { if (e.room == a && enterStep < 0) enterStep = stp; });
        sim.events.On<RoomEvent>("roomClear", e => { if (e.room == a && !res.cleared) { res.cleared = true; res.clearS = (stp - enterStep) * sim.step; } });
        sim.events.On<RoomEvent>("roomRetry", e => { if (e.room == a) res.deaths++; });
        sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room == a) res.falls++; });
        // NIGHT70 Z2 (slate Z2; scout.md fact 3): the quench / pit return emits fallReturn43 (Campaign24.cs:1038), not stageRespawn: counted as a
        // fall in the room, as n46flow counts it (Harness.N46.Flow.cs:234); print only (the run's decisions never read res.falls)
        sim.events.On<FallReturn43Event>("fallReturn43", e => { if (InRoom()) res.falls++; });
        // NIGHT70 Z2: the throw chart, recorded on the plain verb-aware run only (print only: it reads launch36's flights and events after each step)
        bool chart70 = verbAware && !refuseInside && !commit;
        var fly70 = new List<V70Throw>(); var post70 = new List<V70Throw>(); var bursts70 = new List<Launch36BurstEvent>();
        List<double[]> pits70 = V70Pits(json, roomId);
        if (chart70) sim.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => bursts70.Add(new Launch36BurstEvent { position = e.position, hits = e.hits, kills = e.kills, wall = e.wall }));
        // a launch is charted at its LaunchEvent (in the room, as launch36's count), its start read from the new flight: a throw blocked on its
        // first step starts and ends inside one sim.Step (the 0.3 m throws), which an after-step diff alone never sees
        if (chart70) sim.events.On(Launch36.LaunchEvent, o => { if (InRoom()) V70Launched(sim, fly70, o, stp, res); });
        sim.events.On(Launch36.LaunchEvent, o => { if (InRoom()) res.launches++; });
        // NIGHT66 T1b fix 2 (verdict-T1b-r2.md problem 6): the bodies per charged shot are kept for the KESTREL only (the pierce the refusal stops);
        // a charged shotgun shot is counted in `charged` but never as a pierce use
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (InRoom()) { res.charged++; if (e.gun == "revolver") res.chargedHits.Add(e.hits); } });
        sim.events.On(Parry36.Event, o => { if (InRoom()) res.parries++; });
        sim.events.On("fire", o => { if (InRoom()) res.shots++; });
        sim.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => { if (InRoom() && e.kills > 0) res.launchKills.Add($"({V65F(e.position.x)}, {V65F(e.position.y)}, {V65F(e.position.z)}) kills {e.kills}{(e.wall ? " wall" : "")}"); });
        // NIGHT66 T1b: the planted rooms (forced / null; V66PlantForced's comment)
        Enemy marked = null;
        bool markOpen = true;
        sim.events.On<RoomEvent>("roomRetry", e => { if (e.room == a) { marked = null; markOpen = true; } });
        sim.events.On<KillEvent>("kill", e => { if (marked != null && e.enemy == marked.id) marked = null; }); // the mark is spent (the pool may reuse the body)
        int markedCount = 0;
        List<V66Piece> pieces = V66Pieces(json, level.rooms[a]);
        List<V68Face> rideFaces68 = V68Faces(json); // NIGHT68 I1: the level's ride faces (the per-face ride count)
        foreach (V66Piece pc in pieces) res.touches[pc.key] = 0;
        double capR = cfg.movement.body.capsuleRadius, capH = cfg.movement.body.capsuleHeight, padR = cfg.movement.jumpPad.radius, padH = cfg.movement.jumpPad.triggerHeight;
        Vec3 last = default;
        bool haveLast = false;
        sim.StartRun(seed, null, null);
        if (V66PlantForced || V66PlantNull)
        {
            var bonus0 = sim.loadout.bonus;
            sim.loadout.bonus = (e, w, head) =>
            {
                double m = bonus0 != null ? bonus0(e, w, head) : 1;
                if (V66PlantNull && w == "launch36") return 0;
                if (V66PlantForced && e == marked && marked.alive && w != "launch36") return 0;
                return m;
            };
        }
        sim.mover.Events += e =>
        {
            if (sim.campaign == null || sim.campaign.room != a) return;
            if (e.type == MoverEventType.WallRideStart)
            {
                res.rides++; if (double.IsNaN(res.firstRideS) && enterStep >= 0) { res.firstRideS = (stp - enterStep) * sim.step; res.firstRideAt = sim.mover.position; }
                string fk = V68FaceOf(rideFaces68, sim.mover); res.rideFaces.TryGetValue(fk, out int fn); res.rideFaces[fk] = fn + 1; // NIGHT68 I1: by face
            }
            else if (e.type == MoverEventType.SlideStart) res.slides++;
        };
        bool placed = a == 0, doorFreeOn = a > 0;
        int cap = MovementMath.StepsFor(capS, sim.step);
        int rfLast = sim.husks.acid.reflects;
        // NIGHT68 g7 leak trace (--trace-leak): the game's reflect-path events of each step (Parry42's buffer, the LT window, the swing, the
        // punch and parry36's buffer, the parry), printed when a refused run sends a glob back
        var trace68 = new List<string>();
        long ltOpen68 = -1;
        if (V68TraceLeak)
        {
            foreach (string evn in new[] { "parry42Buffer", "ltParry31Open", "ltParry31Return", "meleeSwing", "punch", "parry", Parry36.Event })
            {
                string en = evn;
                sim.events.On(en, o =>
                {
                    string d = o is MeleeEvent me ? $"{en}({me.weapon}{(me.parry ? ", parry" : "")})" : o is HitEvent he ? $"{en}({he.weapon})" : en;
                    trace68.Add(d);
                    if (en == "ltParry31Open") ltOpen68 = (long)Math.Round(sim.SimTime / sim.step);
                });
            }
        }
        for (; stp < cap * 3 && !res.cleared && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); stp++)
        {
            AutopilotInput.TakeDraft(sim);
            bursts70.Clear();
            sim.Step();
            if (chart70) V70ChartStep(sim, fly70, post70, bursts70, pits70, res, stp, InRoom());
            Campaign24 c = sim.campaign;
            int rf = sim.husks.acid.reflects; // NIGHT66 T1b: the globs reflected in the room's fight (skip v2's parry uses)
            if (rf > rfLast && c != null && c.room == a && c.phase == Campaign24.Phase.Fight)
            {
                res.reflects += rf - rfLast;
                // NIGHT68 g7 leak trace (--trace-leak, print only): a glob sent back in a refused run, with the game events of that step and the
                // filter's last gauntlet presses (which path let it through)
                if (V68TraceLeak && refuse == "parry" && (!verbAware || refuseInside))
                {
                    long now = (long)Math.Round(sim.SimTime / sim.step);
                    Console.WriteLine($"  leak trace seed {seed} {(verbAware ? "refused-in" : "walker")}: a glob went back at {V65F(sim.SimTime)} s; events this step: {(trace68.Count > 0 ? string.Join(", ", trace68) : "none")}; LT window last opened {(ltOpen68 >= 0 ? (now - ltOpen68) + " steps before" : "never")}; the filter's last passed gauntlet press {(filter.lastPassStep68 >= 0 ? filter.lastPassKind68 + " " + (now - filter.lastPassStep68) + " steps before" : "none")}, last refused press {(filter.lastRefusedStep68 >= 0 ? (now - filter.lastRefusedStep68) + " steps before" : "none")}; in hand {sim.loadout.Weapon}");
                }
            }
            trace68.Clear();
            rfLast = rf;
            if (!placed && c != null && c.room >= 0) { c.SetDoorFree29(true); sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg); placed = true; V67PutInAwards(sim, level, a); } // NIGHT67 T1c: the put-in's loadout
            if (c == null || c.room < 0) continue;
            if (placed && doorFreeOn && c.room == a) { c.SetDoorFree29(false); doorFreeOn = false; }
            V67SayEntry(sim, level, a, "skip v2"); // NIGHT67 T1c: a print only
            if (c.room == a && c.phase == Campaign24.Phase.Fight)
            {
                res.fight++;
                if (sim.mover.state == MoveState.WallRide) res.rideSteps++; // NIGHT66 T1b fix: the steps on a wall (a refused run's rides end at once)
                int rank = sim.style == null ? 0 : sim.style.rank;
                if (rank > 0) res.aboveDull++;
                if (rank > res.top) res.top = rank;
                // NIGHT66 T1b: the metres walked (flat; a jump over 3 m in one step is a respawn, not counted), the pieces touched, the mark
                Vec3 p = sim.mover.position;
                if (haveLast) { double dx = p.x - last.x, dz = p.z - last.z, d = Math.Sqrt(dx * dx + dz * dz); if (d < 3) res.walked += d; }
                last = p; haveLast = true;
                foreach (V66Piece pc in pieces) if (V66Touch(pc, p, capR, capH, padR, padH)) res.touches[pc.key]++;
                if (V66PlantForced && markOpen)
                {
                    RoomDef rd = level.rooms[a];
                    Enemy m = sim.enemies.all.FirstOrDefault(e => e.alive && e is Husk && !e.standIn29 && !e.tank29 && e.position.x >= rd.x0 - 1 && e.position.x <= rd.x1 + 1 && e.position.z >= rd.z0 - 1 && e.position.z <= rd.z1 + 1);
                    if (m != null) { marked = m; markOpen = false; markedCount++; }
                }
            }
            else haveLast = false;
            if (enterStep >= 0 && stp - enterStep > cap) break;
        }
        ap.Detach();
        if (chart70) V70ChartEnd(sim, fly70, post70, res); // NIGHT70 Z2: a flight still running, or a body still tracked, at the run's end
        res.innerRefused = filter.refusedPresses;
        res.rideSteered = filter.rideSteered; res.rideDashes = filter.rideDashes; res.rideEnded = filter.rideEnded; res.ridesCaught = filter.ridesCaught;
        res.rideContacts = filter.rideContacts; // NIGHT68 I1
        res.declined = vp != null && refuseInside ? vp.refusedEpisodes : 0;
        res.gauntletRefused = filter.gauntletRefused; res.handAtEnd = sim.loadout.Weapon; // NIGHT68 g7
        res.refused = res.innerRefused + res.declined;
        if (vp != null && V65Stats.TryGetValue(roomId, out V65Stat st)) { res.trigger = st.trigger; res.verbRefused = st.refused; if (st.verb.StartsWith("launch")) res.launchLine = V68LaunchLine(st); res.offlineCut = st.offlineCut; res.lineFace = st.lineFace; res.refusedWhy = st.refusedWhy; res.swingParries = st.swingParries; res.commitHeld = st.commitHeld; res.commitClose = st.commitClose; res.commitKept = st.commitKept; res.commitSwitch = st.commitSwitch; res.commitBad = st.commitBad; }
        if (V66PlantForced) res.plantNote = $"planted forced: {markedCount} body marked, {(marked != null && marked.alive ? "the mark ALIVE at the end" : "no mark alive at the end")}";
        return res;
    }

    // ================================================================ skip v2 (NIGHT66 T1b; FORGE66 D-4, R-3, redteam.md finding 3)
    /// <summary>Skip v2: three runs per seed (10 by default) of the room from its spawn, the same walk pilot under each:
    ///   verb-aware   the verb-aware pilot (V65VerbPilot), the verb allowed
    ///   refused-in   the SAME pilot with the verb refused inside it (V65VerbPilot.refuse: its episodes decline the verb and release its holds
    ///                and approach, so it plays on as the walker; V65Filter round it drops the inner pilot's own verb presses)
    ///   walker       the plain walk pilot with the verb's presses dropped (v1's refused run)
    /// Per seed: the three clear times (a run that does not clear in the cap counts the cap), deaths, the verb's uses, STYLE, metres walked and
    /// shots fired (the refused run moved and fired), the presses refused, the paired ratio min(refused-in, walker) / verb-aware, exercised or
    /// not (the verb-aware run used the verb AND the refused-in run refused a press), the pieces the verb-aware run touched and launch36's kills.
    /// The verdict: `n65verb skip: M1 PASS|FAIL|UNSURE | room <id>, ratio <x> vs bar <b>, exercised <n>/<seeds>`: the median of the exercised
    /// seeds' ratios against the room's mainBar (`--bars <slate.json>`, else `--bar <b>`, else 1.25), UNSURE under 7 of 10 exercised. Then
    /// the STYLE+15 line (NIGHT67's rule, printed only), the M2 VERB line (NIGHT68 I4; its STYLE part NIGHT69 Y1: verb-aware STYLE above DULL
    /// >= the faster refusal's - 5 points, pooled over the exercised seeds), the touch table and the kills.
    /// `--v1` runs NIGHT65's skip (two runs per seed, V65SkipV1).</summary>
    static int V66SkipV2(string[] args)
    {
        if (Array.IndexOf(args, "--style-cases") >= 0) return V68StyleCases(); // NIGHT68 I4: the STYLE rule's planted cases (no sim)
        // NIGHT68 I4: --room-file <file>: its sha256 printed as the fresh pre-run's header line (the GATE pack takes the file by --fresh only when
        // it equals the round's record): the file's bytes decoded as UTF-8 (a BOM kept as U+FEFF), every CR LF made LF, encoded as UTF-8
        string roomFile = N53RkOpt(args, "--room-file", null);
        if (roomFile != null)
        {
            if (!File.Exists(Path.GetFullPath(roomFile))) { Console.WriteLine($"n65verb skip v2: REFUSED: --room-file {roomFile}: no such file"); return 2; }
            Console.WriteLine($"# room-file sha256: {V68Sha256N(File.ReadAllBytes(Path.GetFullPath(roomFile)))} ({roomFile.Replace('\\', '/')}; the GATE pack's normalisation: UTF-8, CR LF -> LF)");
        }
        string path = N53RkOpt(args, "--level", "Design/levels/n52-foundry.json");
        // NIGHT68 I4 fix (c) (verdict-I4.md P3): the pre-run tied to the level file it played as well: its sha256, the same normalisation
        if (roomFile != null && File.Exists(Path.GetFullPath(path)))
            Console.WriteLine($"# level sha256: {V68Sha256N(File.ReadAllBytes(Path.GetFullPath(path)))} ({path.Replace('\\', '/')}; the level this skip played)");
        string roomId = N53RkOpt(args, "--room", "a5");
        V65ParsePlants(N53RkOpt(args, "--plant-verb65", null));
        V68ParseOnly(N53RkOpt(args, "--plant-only", null)); // NIGHT68 I2: a room's waves planted as one kind (the planted swarmer-only room)
        V68ParseLimit(N53RkOpt(args, "--plant-limit", null)); // NIGHT68 g7: a room's `limit` row field planted (g7 melee only, in memory)
        V68TraceLeak = Array.IndexOf(args, "--trace-leak") >= 0; // NIGHT68 g7: print each glob a refused run sends back, with its path
        V66PlantForced = Array.IndexOf(args, "--plant-forced") >= 0;
        V66PlantNull = Array.IndexOf(args, "--plant-null") >= 0;
        V67PutInOff = Array.IndexOf(args, "--no-put-in-awards") >= 0; // NIGHT67 T1c: the old put-in (the planted RED)
        V66ParseSets(args);
        foreach (string kv in V66Sets) Console.WriteLine($"n65verb skip v2: --set {kv} (this process's config only: a planted case)");
        string verb65 = null;
        var jl = V65Planted(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        foreach (object o in PortData.Arr(PortData.Obj(jl["campaign"])["rooms"])) { var rr = PortData.Obj(o); if ((string)rr["id"] == roomId && rr.TryGetValue("verb65", out object vv)) verb65 = vv as string; }
        int seeds = int.Parse(N53RkOpt(args, "--seeds", "10"), V65Ci);
        uint first = uint.Parse(N53RkOpt(args, "--seed", "1"), V65Ci);
        double cap = double.Parse(N53RkOpt(args, "--cap", "120"), V65Ci);
        int need = (int)Math.Ceiling(0.7 * seeds); // 7 of 10
        // the bar: --bar, else the slate's mainBar for this room (--bars), else 1.25
        double bar = 1.25;
        string barFrom = "the default 1.25 (no --bars / --bar)";
        // NIGHT66 T1b fix (the critic's problem 2): a bar that is not the room's own mainBar says so ON the M1 line
        string barNote = " (default: no --bars)";
        string barsArg = N53RkOpt(args, "--bars", null), barArg = N53RkOpt(args, "--bar", null);
        if (barArg != null) { bar = double.Parse(barArg, V65Ci); barFrom = "--bar " + barArg; barNote = " (--bar)"; }
        else if (barsArg != null)
        {
            if (!File.Exists(Path.GetFullPath(barsArg))) { Console.WriteLine($"n65verb skip v2: REFUSED: --bars {barsArg}: no such file (from {Directory.GetCurrentDirectory()})"); return 2; }
            barFrom = $"the default 1.25 ({roomId} has no slice in {barsArg})";
            barNote = " (no slice for the room in the slate)";
            foreach (object o in PortData.Arr(PortData.Obj(PortData.LoadJson(Path.GetFullPath(barsArg)))["slices"]))
            {
                var s = PortData.Obj(o);
                if (!(s.TryGetValue("id", out object io) && io as string == roomId)) continue;
                if (s.TryGetValue("mainBar", out object mb) && mb is double d) { bar = d; barFrom = $"{barsArg} {roomId}.mainBar"; barNote = " (mainBar)"; }
                else { barFrom = $"the default 1.25 ({barsArg} {roomId}.mainBar is null)"; barNote = " (mainBar null)"; }
            }
        }
        string Line(string verdict, double ratio, int nEx) => $"n65verb skip: M1 {verdict} | room {roomId}, ratio {(double.IsNaN(ratio) ? "-" : ratio.ToString("0.00", V65Ci))} vs bar {bar.ToString("0.00", V65Ci)}{barNote}, exercised {nEx}/{seeds}";
        if (verb65 == null)
        {
            Console.WriteLine($"n65verb skip v2: level {path}, room {roomId}: no verb65 in the row (and none planted): the verb-aware pilot has no verb to use");
            Console.WriteLine(Line("UNSURE", double.NaN, 0) + " | no verb65: nothing to refuse");
            return 3;
        }
        string fromVerb = verb65.StartsWith("launch") ? "launch" : verb65;
        string refuse = N53RkOpt(args, "--refuse", fromVerb);
        if ((V66PlantForced || V66PlantNull) && refuse != "launch") { Console.WriteLine($"n65verb skip v2: REFUSED: --plant-forced / --plant-null take a launch room (this one refuses {refuse})"); return 2; }
        // NIGHT70 Z3 (slate Z3; DECIDE70 D-5): in a launch room skip v2 runs a FOURTH run per seed, the committed run (V65VerbPilot.commit: the
        // verb-aware run holding fire on launchable bodies inside 12 m and closing on them), after the other three (which run as before), and the
        // verb side of each seed is the faster of the plain verb-aware run and the committed run (as the refusal is the faster of two): the
        // ratio, exercised, STYLE and deaths are read on the run chosen. `--no-commit`: the plain build's skip v2 (no fourth run, every line as
        // before); `--commit` is the default in a launch room (said on a room of another verb: there is no committed run for it).
        bool noCommit70 = Array.IndexOf(args, "--no-commit") >= 0;
        bool doCommit = refuse == "launch" && !noCommit70;
        V70CommitBurstCap = Array.IndexOf(args, "--commit-burst-cap") >= 0; // round 4: the option, off by default (the literal rule)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine($"n65verb skip v2: level {path}, room {roomId}, verb65 {verb65}{(V65Plants.ContainsKey(roomId) ? " (planted, bot43 false)" : "")}, refuse {refuse}, seeds {first}..{first + seeds - 1} (--seeds {seeds}), cap {V65F(cap)} s, bar {bar.ToString("0.00", V65Ci)} from {barFrom}, need {need} exercised"
            + $"{(V66PlantForced ? "; PLANTED forced: the room's first launchable husk takes damage only from launch36" : "")}{(V66PlantNull ? "; PLANTED null: launch36's damage x0" : "")}");
        Console.WriteLine("  runs: verb-aware = Walk38Pilot + n46flow's lava rule + the ported autopilot (normal) + the verb-aware layer; refused-in = the same with the verb refused inside the layer (episodes declined, holds and approach released) and the inner pilot's verb presses dropped; walker = the plain pilot with the verb's presses dropped. Not invulnerable. A run that does not clear counts the cap.");
        if (doCommit) Console.WriteLine("  runs: committed (NIGHT70 Z3) = the verb-aware run that, while no launch episode runs and a launchable body (husk-shaped, not a stand-in or a tank, hp above the launch bound" + (V70CommitBurstCap ? "; OPTION --commit-burst-cap ON: only bodies at or under launch36's burst damage, a body its launch ends" : "; the slate's literal rule, chargers included (--commit-burst-cap off)") + ") is inside 12 m, holds its fire (a pistol / shotgun charge already building kept held, never released) and closes on the nearest such body; the verb side of each seed = the faster of verb-aware and committed (ties: verb-aware); ratio, exercised, STYLE and deaths read on that run; the card's launch target is the verb-aware (plain) run's launch36. --no-commit: the plain build's skip v2.");
        else if (Array.IndexOf(args, "--commit") >= 0) Console.WriteLine($"  runs: --commit: no committed run in a room that refuses {refuse}{(noCommit70 ? " (--no-commit given)" : "")} (NIGHT70 Z3: the committed run is a launch room's)");
        // NIGHT66 T1b fix (the critic's problem 5): a pierce use is a charged shot that hit 2 or more bodies (the verb a2 / a8 plan for), not
        // any charged shot (the autopilot's own charges, one body or none, counted before)
        int UsesOf(V65Clear c) => refuse == "launch" ? c.launches : refuse == "pierce" ? c.chargedHits.Count(h => h >= 2) : refuse == "parry" ? c.reflects : refuse == "slide" ? c.slides : c.rides;
        string usesName = refuse == "launch" ? "launch36" : refuse == "pierce" ? "pierces (charged shots through >= 2 bodies)" : refuse == "parry" ? "globs parried" : refuse == "slide" ? "slides" : "wall rides";
        double T(V65Clear c) => c.cleared ? c.clearS : cap;
        double Dull(V65Clear c) => c.fight > 0 ? 100.0 * c.aboveDull / c.fight : 0;
        string Run(V65Clear c) => $"cleared {(c.cleared ? "yes in " + V65F(c.clearS) + " s" : "NO (cap " + V65F(cap) + " s)")} | deaths {c.deaths}, falls {c.falls} | {usesName} {UsesOf(c)} | STYLE above DULL {Dull(c).ToString("0.0", V65Ci)} %, top {F56RankNames[Math.Max(0, Math.Min(F56Ranks - 1, c.top))]} | walked {V65F(c.walked)} m, shots {c.shots} (charged {c.charged}{(c.chargedHits.Count > 0 ? ", bodies hit per KESTREL charged shot " + string.Join("/", c.chargedHits) : "")})"
            + (refuse == "parry" ? $" | parry36 events {c.parries} (melee parries too) | in hand at the room's clear {c.handAtEnd ?? "-"}, gauntlet presses refused {c.gauntletRefused}, the pilot's swing parry presses {c.swingParries}" : "") // NIGHT68 g7
            + (refuse == "ride" ? $" | first ride {(double.IsNaN(c.firstRideS) ? "none" : V65F(c.firstRideS) + " s at " + V65P(c.firstRideAt))} | rides by face: {V68FaceCounts(c.rideFaces)}" : "");
        var A = new List<V65Clear>(); var B = new List<V65Clear>(); var C = new List<V65Clear>();
        var ratios = new List<double>(); var exercised = new List<bool>();
        var V = new List<V65Clear>(); var plainRatios = new List<double>(); var plainEx = new List<bool>(); var commitSeeds = new List<string>(); var D = new List<V65Clear>(); // NIGHT70 Z3
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            V65Clear a = V65ClearRun(path, roomId, seed, refuse, cap, true, false);
            V65Clear b = V65ClearRun(path, roomId, seed, refuse, cap, true, true);
            V65Clear c = V65ClearRun(path, roomId, seed, refuse, cap, false, false);
            V65Clear d = doCommit ? V65ClearRun(path, roomId, seed, refuse, cap, true, false, true) : null; // NIGHT70 Z3: the committed run, after the three
            V65Clear v = d != null && T(d) < T(a) ? d : a; // NIGHT70 Z3: the verb side, the faster of the two verb-aware runs (ties: the plain one)
            A.Add(a); B.Add(b); C.Add(c); V.Add(v); if (d != null) D.Add(d);
            if (v == d) commitSeeds.Add(seed.ToString(V65Ci));
            bool ex = UsesOf(v) > 0 && b.refused > 0;
            exercised.Add(ex);
            bool inFaster = T(b) <= T(c);
            double ratio = Math.Min(T(b), T(c)) / T(v);
            ratios.Add(ratio);
            plainRatios.Add(Math.Min(T(b), T(c)) / T(a)); plainEx.Add(UsesOf(a) > 0 && b.refused > 0);
            Console.WriteLine($"  seed {seed} verb-aware: {Run(a)} | trigger held {a.trigger} steps{(a.plantNote != null ? " | " + a.plantNote : "")}{(a.launchLine != null ? " | " + a.launchLine : "")}"
                + (refuse == "ride" ? $" | the ride face {a.lineFace ?? "-"}; off-line ride starts cut {a.offlineCut} (wall contacts under the noride rule outside the line episode)" : "")
                + (a.verbRefused > 0 ? $" | REFUSED: {a.refusedWhy ?? "the verb-aware pilot does not perform " + verb65} ({a.verbRefused} steps)" : "")); // NIGHT68 I1
            // NIGHT68 I1: the refusal now stops the ride before it starts (the room under the game's noride rule); the backstop is its leak detector
            string rideCounts(V65Clear q) => refuse == "ride" ? $" (the ride refusal: the room under the game's noride rule, wall contacts in the air it refused {q.rideContacts}; the backstop, its leaks: rides started and ended by the away rule {q.ridesCaught}, steps the stick was turned away {q.rideEnded}, steps on a wall {q.rideSteps}; every other input the walker's own)" : "";
            Console.WriteLine($"  seed {seed} refused-in: {Run(b)} | presses refused {b.refused} (episodes declined {b.declined}, the inner pilot's presses dropped {b.innerRefused}){rideCounts(b)}{(UsesOf(b) > 0 ? $" | WARN the refused run used the verb {UsesOf(b)} time(s)" : "")}{(b.walked < 5 || b.shots == 0 ? " | WARN the refused run hardly moved or never fired" : "")}{(b.plantNote != null ? " | " + b.plantNote : "")}");
            Console.WriteLine($"  seed {seed} walker:     {Run(c)} | presses refused {c.refused}{rideCounts(c)}{(UsesOf(c) > 0 ? $" | NOTE the walker used the verb {UsesOf(c)} time(s) past the filter" : "")}{(c.plantNote != null ? " | " + c.plantNote : "")}");
            // NIGHT70 Z3: the committed run's line (a new line kind: `seed N committed:`, so the plain run's `verb-aware:` line stays the card's launch count)
            if (d != null)
                Console.WriteLine($"  seed {seed} committed:  {Run(d)} | trigger held {d.trigger} steps{(d.launchLine != null ? " | " + d.launchLine : "")} | fire held {d.commitHeld} steps (a launchable body inside 12 m: hp above the launch bound{(V70CommitBurstCap ? " and at or under launch36's burst (--commit-burst-cap)" : ", chargers included (the literal rule)")}; no episode running), of them a building charge kept held {d.commitKept}, a switch pistol <-> shotgun asked to drop a charge or a queued tap {d.commitSwitch}, closed on it {d.commitClose} steps, gun shots in a held step {d.commitBad}{(d.commitBad > 0 ? " (FAULT: the committed run fired while holding)" : "")}");
            string why = ex ? "exercised" : "NOT exercised (" + string.Join(", ", new[] { UsesOf(v) == 0 ? (d != null ? "the chosen verb-aware run never used the verb" : "the verb-aware run never used the verb") : null, b.refused == 0 ? "the refused run refused no press" : null }.Where(q => q != null)) + "; not counted)";
            string vside = d != null ? $"verb side {(v == d ? "committed" : "verb-aware")} {V65F(T(v))} s (verb-aware {V65F(T(a))} s, committed {V65F(T(d))} s; the verb-aware run alone: ratio {plainRatios[s].ToString("0.00", V65Ci)})" : $"verb-aware {V65F(T(a))} s";
            Console.WriteLine($"  seed {seed} => ratio {ratio.ToString("0.00", V65Ci)} (the faster refusal: {(inFaster ? "refused-in" : "walker")} {V65F(Math.Min(T(b), T(c)))} s / {vside}) | {why} | refused {refuse} {b.refused} (the refused-in run's presses refused and episodes declined), the refused-in run used the verb {UsesOf(b)}, the walker {UsesOf(c)}{(ex && (UsesOf(b) > 0 || UsesOf(c) > 0) ? " | LEAK: the refused run used the verb" : "")}"); // NIGHT68 I4: the ROOM list's `refused <verb> N` per seed; the leak (verdict-I4.md P2)
            var touched = a.touches.Where(kv => kv.Value > 0).Select(kv => $"{kv.Key} {kv.Value}").ToList();
            Console.WriteLine($"  seed {seed} touches (verb-aware, steps): {(touched.Count > 0 ? string.Join("; ", touched) : "none")}{(a.launchKills.Count > 0 ? " | launch36 kills: " + string.Join("; ", a.launchKills) : "")}");
        }
        var exR = ratios.Where((r, i) => exercised[i]).OrderBy(r => r).ToList();
        int nEx = exR.Count;
        double median = nEx == 0 ? double.NaN : nEx % 2 == 1 ? exR[nEx / 2] : (exR[nEx / 2 - 1] + exR[nEx / 2]) / 2;
        string verdict = nEx < need ? "UNSURE" : median >= bar ? "PASS" : "FAIL";
        string tail = nEx < need ? $" | too few seeds exercised ({nEx} < {need}): the comparison does not test the verb" : "";
        Console.WriteLine(Line(verdict, median, nEx) + tail);
        // M2: STYLE above DULL pooled over the exercised seeds, the verb-aware run against the faster refusal of each seed
        long fa = 0, ua = 0, fr = 0, ur = 0;
        for (int i = 0; i < seeds; i++)
        {
            if (!exercised[i]) continue;
            fa += V[i].fight; ua += V[i].aboveDull; // NIGHT70 Z3: the verb side the seed's ratio chose (no committed run: the verb-aware run)
            V65Clear r = T(B[i]) <= T(C[i]) ? B[i] : C[i];
            fr += r.fight; ur += r.aboveDull;
        }
        double sa = fa > 0 ? 100.0 * ua / fa : double.NaN, sr = fr > 0 ? 100.0 * ur / fr : double.NaN;
        // NIGHT70 Z3 critic P3 (print only): the verb-aware run alone's STYLE above DULL, pooled over ITS exercised seeds (the plain run's own
        // M2 STYLE), printed beside the chosen run's on the M2 VERB line when the committed run ran
        long pfa = 0, pua = 0;
        for (int i = 0; i < seeds; i++) if (plainEx[i]) { pfa += A[i].fight; pua += A[i].aboveDull; }
        double spl = pfa > 0 ? 100.0 * pua / pfa : double.NaN;
        string plainStyle = doCommit ? $" (plain STYLE {(double.IsNaN(spl) ? "-" : spl.ToString("0.0", V65Ci))} %: the verb-aware run alone, over its {plainEx.Count(x => x)} exercised seeds)" : "";
        string m2 = nEx == 0 ? "UNSURE" : sa >= sr + 15 ? "PASS" : "FAIL";
        // NIGHT68 I4: NIGHT67's STYLE rule (+15 absolute) kept beside the new one under its own name (the M2 VERB line below is the verdict)
        Console.WriteLine($"n65verb skip: STYLE+15 {m2} | room {roomId}, STYLE above DULL verb-aware {(double.IsNaN(sa) ? "-" : sa.ToString("0.0", V65Ci))} % vs the faster refusal {(double.IsNaN(sr) ? "-" : sr.ToString("0.0", V65Ci))} % (+15 needed: NIGHT67's rule, not the verdict), over the {nEx} exercised seed(s)");
        // ---- NIGHT68 I4 (slate I4 line 1; the ROOM list's M2 VERB): the pooled ratio (the median of the exercised seeds' ratios over every set),
        // each set of 10 seeds' median beside it, `exercised N / <seeds>` (need 70 %: 14 of 20), the refusal fired on every exercised seed
        // (`refused <verb> N`, N > 0: the seed lines), STYLE above DULL on the verb-aware run >= the faster refusal's - 5 (NIGHT69 Y1; NaN:
        // UNSURE; NIGHT68's min(refusal + 15, max(refusal, 95)) printed beside it, not the verdict), and a ride room's ratio inside [mainBar,
        // mainBar + 0.10) UNSURE. With --seed 21 and up (the GATE's fresh seeds): the M2 FRESH line, median >= 1.25 with >= 7 of 10 exercised.
        double MedOf(List<double> v) { var l = v.OrderBy(q => q).ToList(); return l.Count == 0 ? double.NaN : l.Count % 2 == 1 ? l[l.Count / 2] : (l[l.Count / 2 - 1] + l[l.Count / 2]) / 2; }
        string R2(double v) => double.IsNaN(v) ? "-" : v.ToString("0.00", V65Ci);
        var sets = new List<string>();
        for (int s0 = 0; s0 < seeds; s0 += 10)
        {
            int s1 = Math.Min(seeds, s0 + 10);
            var ex = Enumerable.Range(s0, s1 - s0).Where(i => exercised[i]).Select(i => ratios[i]).ToList();
            sets.Add($"{first + s0}-{first + s1 - 1} {R2(MedOf(ex))} ({ex.Count}/{s1 - s0})");
        }
        double medAll = MedOf(ratios);
        int fired = Enumerable.Range(0, seeds).Count(i => exercised[i] && B[i].refused > 0); // (exercised requires it: printed, never the cause)
        // NIGHT68 I4 fix (Logs/n68/verdict-I4.md P2): the LEAK judged: an exercised seed whose refused runs (refused-in or walker) still used the
        // verb fails the line (NIGHT67's "the ride refusal leaked on 10 / 10"); each such seed is named
        var leakSeeds = Enumerable.Range(0, seeds).Where(i => exercised[i] && (UsesOf(B[i]) > 0 || UsesOf(C[i]) > 0)).Select(i => (first + i).ToString(V65Ci)).ToList();
        int leaked = leakSeeds.Count;
        // NIGHT69 Y1: the STYLE need is the faster refusal's - 5 (V68StyleNeed); NaN (nothing to judge) is UNSURE; NIGHT68's need printed beside
        double styleNeed = V68StyleNeed(sr), styleNeedN68 = V68StyleNeedN68(sr);
        string styleV = V69StyleVerdict(sa, sr);
        bool rideBand = refuse == "ride" && !double.IsNaN(median) && median >= bar && median < bar + 0.10;
        string vm2 = nEx < need ? "UNSURE" : median < bar ? "FAIL" : fired < nEx ? "FAIL" : leaked > 0 ? "FAIL" : styleV == "FAIL" ? "FAIL" : styleV == "UNSURE" ? "UNSURE" : rideBand ? "UNSURE" : "PASS";
        string why2 = nEx < need ? $"too few seeds exercised ({nEx} < {need})" : median < bar ? "the pooled ratio is under the bar" : fired < nEx ? "the refusal did not fire on every exercised seed" : leaked > 0 ? $"the refused run still used the verb on {leaked} exercised seed(s) ({string.Join(", ", leakSeeds)})" : styleV == "FAIL" ? "STYLE under its need (the faster refusal's - 5)" : styleV == "UNSURE" ? "STYLE not measured (no fight on the exercised seeds)" : rideBand ? "a ride room inside [mainBar, mainBar + 0.10)" : "every part met";
        string verdict68 = vm2; // NIGHT68 I4 fix (P1): the exit code follows the M2 VERB line (seeds 1+), or M2 FRESH (seeds 21+)
        // NIGHT70 Z3: the committed run's part of the M2 line (printed only when it ran): the seeds that took it, the verb-aware run alone's pooled
        // ratio and exercised count (red team B3: the selection gain shown), deaths and launch36 on the chosen runs against the verb-aware run's
        string z3 = "";
        if (doCommit)
        {
            var pex = Enumerable.Range(0, seeds).Where(i => plainEx[i]).Select(i => plainRatios[i]).ToList();
            z3 = $"; the verb side per seed = the faster of verb-aware and committed ({(V70CommitBurstCap ? "OPTION --commit-burst-cap: held only for bodies at or under launch36's burst" : "the literal rule: held for every husk-shaped body above the launch bound")}): committed on {commitSeeds.Count} of {seeds} seeds ({(commitSeeds.Count > 0 ? string.Join(", ", commitSeeds) : "none")}), the verb-aware run alone: ratio {R2(MedOf(pex))} with {pex.Count} / {seeds} exercised, deaths on the chosen runs {V.Sum(q => q.deaths)} (verb-aware {A.Sum(q => q.deaths)}, committed {D.Sum(q => q.deaths)}), launch36 per run median verb-aware {V65F(MedOf(A.Select(q => (double)UsesOf(q)).ToList()))} / committed {V65F(MedOf(D.Select(q => (double)UsesOf(q)).ToList()))} (the card's target reads verb-aware), gun shots in a committed held step {D.Sum(q => q.commitBad)}";
        }
        if (first < 21)
            Console.WriteLine($"n65verb skip: M2 VERB {vm2} | room {roomId}, verb {refuse}, pooled ratio {R2(median)} vs mainBar {bar.ToString("0.00", V65Ci)}{barNote}, sets {string.Join(", ", sets)}, exercised {nEx} / {seeds} (need {need}), median of all {seeds} seeds {R2(medAll)}, refusal fired on {fired} of {nEx} exercised seeds (by construction: an exercised seed refused a press), the refused run used the verb on {leaked} exercised seed(s){(leaked > 0 ? " (" + string.Join(", ", leakSeeds) + ")" : "")}, STYLE above DULL {(double.IsNaN(sa) ? "-" : sa.ToString("0.0", V65Ci))} %{plainStyle} vs need {(double.IsNaN(styleNeed) ? "-" : styleNeed.ToString("0.0", V65Ci))} % (the faster refusal {(double.IsNaN(sr) ? "-" : sr.ToString("0.0", V65Ci))} % - 5: STYLE {styleV}; NIGHT68's min(refusal + 15, max(refusal, 95)) {(double.IsNaN(styleNeedN68) ? "-" : styleNeedN68.ToString("0.0", V65Ci))} %, not the verdict){(refuse == "ride" ? $", ride band [{bar.ToString("0.00", V65Ci)}, {(bar + 0.10).ToString("0.00", V65Ci)}) UNSURE" : "")}{z3} | {why2}");
        else
        {
            // NIGHT68 I4 fix (P5): on the GATE's fresh seeds only the ROOM list's fresh test is printed (no M2 VERB line to misread)
            string fm = nEx < need ? "UNSURE" : median >= 1.25 ? "PASS" : "FAIL";
            Console.WriteLine($"n65verb skip: M2 FRESH {fm} | room {roomId}, seeds {first}..{first + seeds - 1} (never tuned on), median {R2(median)} vs 1.25, exercised {nEx} / {seeds} (need {need}); the refused run used the verb on {leaked} exercised seed(s){(leaked > 0 ? " (" + string.Join(", ", leakSeeds) + ")" : "")} (printed, not judged: the ROOM list's fresh test is the median and the exercised count){z3}");
            verdict68 = fm;
        }
        double Med(IEnumerable<double> v) { var l = v.OrderBy(q => q).ToList(); return l.Count == 0 ? double.NaN : l.Count % 2 == 1 ? l[l.Count / 2] : (l[l.Count / 2 - 1] + l[l.Count / 2]) / 2; }
        Console.WriteLine($"n65verb skip: totals | room {roomId} | median clear (all seeds; the cap for a non-clear): verb-aware {V65F(Med(A.Select(T)))} s, refused-in {V65F(Med(B.Select(T)))} s, walker {V65F(Med(C.Select(T)))} s"
            + $" | cleared {A.Count(c => c.cleared)} / {B.Count(c => c.cleared)} / {C.Count(c => c.cleared)} of {seeds} | deaths {A.Sum(c => c.deaths)} / {B.Sum(c => c.deaths)} / {C.Sum(c => c.deaths)}"
            + $" | {usesName} {A.Sum(UsesOf)} / {B.Sum(UsesOf)} / {C.Sum(UsesOf)} | presses refused (refused-in) {B.Sum(c => c.refused)}, (walker) {C.Sum(c => c.refused)}"
            + $" | walked (median m) {V65F(Med(A.Select(c => c.walked)))} / {V65F(Med(B.Select(c => c.walked)))} / {V65F(Med(C.Select(c => c.walked)))} | shots (median) {V65F(Med(A.Select(c => (double)c.shots)))} / {V65F(Med(B.Select(c => (double)c.shots)))} / {V65F(Med(C.Select(c => (double)c.shots)))}"
            + (doCommit ? $" | committed (NIGHT70 Z3): median clear {V65F(Med(D.Select(T)))} s, cleared {D.Count(c => c.cleared)} of {seeds}, deaths {D.Sum(c => c.deaths)}, launch36 {D.Sum(UsesOf)}, chosen on {commitSeeds.Count} seeds" : "")
            + $" | wall time {sw.Elapsed.TotalSeconds.ToString("0", V65Ci)} s");
        // the touch table (M4's test a, read by Tools/night/n66-deco.cjs): every piece of the room, the seeds whose verb-aware run touched it
        List<V66Piece> pcs = V66Pieces(jl, LevelDef.From(jl).rooms.First(x => x.id == roomId));
        if (A.Count > 0)
            foreach (string key in A[0].touches.Keys)
            {
                int k = A.Count(c => c.touches.TryGetValue(key, out int n) && n > 0);
                long steps = A.Sum(c => c.touches.TryGetValue(key, out int n) ? n : 0);
                string label = pcs.FirstOrDefault(p => p.key == key)?.label ?? "-";
                Console.WriteLine($"n65verb skip touch: room {roomId} | {key} | seeds {k}/{seeds} | steps {steps} | {label}");
            }
        for (int i = 0; i < seeds; i++)
            foreach (string k in A[i].launchKills) Console.WriteLine($"n65verb skip launchkill: room {roomId} | seed {first + i} | at {k}");
        // NIGHT70 Z2: the throw chart (a NEW line kind; the area lint and the deco checker read launchkill), the plain verb-aware run's launches
        for (int i = 0; i < seeds; i++)
            foreach (string k in A[i].launchEnds) Console.WriteLine($"n65verb skip launchend: room {roomId} | seed {first + i} | {k}");
        return verdict68 == "PASS" ? 0 : verdict68 == "FAIL" ? 1 : 3; // NIGHT68 I4 fix (P1): M2 VERB / M2 FRESH, not M1 (M1 stays printed)
    }

    static int V65SkipV1(string[] args)
    {
        V66ParseSets(args); // NIGHT66 T1b fix 2: the planted rideless room (--set) for the walker's none / ride pair
        foreach (string kv in V66Sets) Console.WriteLine($"n65verb skip --v1: --set {kv} (this process's config only: a planted case)");
        string path = N53RkOpt(args, "--level", "Design/levels/n52-foundry.json");
        string roomId = N53RkOpt(args, "--room", "a5");
        var dflt = new Dictionary<string, string> { ["a2"] = "pierce", ["a8"] = "pierce", ["a4"] = "parry", ["a5"] = "launch", ["b1"] = "launch", ["a3"] = "ride" };
        // NIGHT65 T: --verbs65 (the verb-aware pilot in the allowed run) and --plant-verb65 (rows planted in memory, with bot43 false)
        bool verbAware = Array.IndexOf(args, "--verbs65") >= 0;
        V65ParsePlants(N53RkOpt(args, "--plant-verb65", null));
        string verb65 = null;
        {
            var j = V65Planted(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
            foreach (object o in PortData.Arr(PortData.Obj(j["campaign"])["rooms"])) { var rr = PortData.Obj(o); if ((string)rr["id"] == roomId && rr.TryGetValue("verb65", out object vv)) verb65 = vv as string; }
        }
        string fromVerb = verb65 == null ? null : verb65.StartsWith("launch") ? "launch" : verb65;
        string refuse = N53RkOpt(args, "--refuse", fromVerb ?? (dflt.TryGetValue(roomId, out string d) ? d : "launch"));
        int seeds = int.Parse(N53RkOpt(args, "--seeds", "3"), V65Ci);
        uint first = uint.Parse(N53RkOpt(args, "--seed", "1"), V65Ci);
        double cap = double.Parse(N53RkOpt(args, "--cap", "120"), V65Ci);
        Console.WriteLine($"n65verb skip: level {path}, room {roomId}, verb65 {(verb65 ?? "none")}{(V65Plants.ContainsKey(roomId) ? " (planted, bot43 false)" : "")}, refuse {refuse}, seeds {first}..{first + seeds - 1}, cap {V65F(cap)} s; pilot Walk38Pilot + n46flow's lava rule + the ported autopilot (normal){(verbAware ? " + the verb-aware layer (--verbs65) in the allowed run" : "")}, not invulnerable");
        if (verbAware && verb65 == null) Console.WriteLine($"  NOTE: --verbs65 but {roomId}'s row has no verb65: the allowed run is the plain pilot");
        var on = new List<V65Clear>(); var off = new List<V65Clear>();
        string Style(V65Clear c) => $"above DULL {(c.fight > 0 ? 100.0 * c.aboveDull / c.fight : 0).ToString("0.0", V65Ci)} %, top {F56RankNames[Math.Max(0, Math.Min(F56Ranks - 1, c.top))]}";
        int UsesOf(V65Clear c) => refuse == "launch" ? c.launches : refuse == "pierce" ? c.charged : refuse == "parry" ? c.parries : refuse == "slide" ? c.slides : c.rides;
        string uses(V65Clear c) => (refuse == "launch" ? "launch36 " : refuse == "pierce" ? "charged shots " : refuse == "parry" ? "parries " : refuse == "slide" ? "slides " : "wall rides ") + UsesOf(c);
        // NIGHT65 (the S GATE critic, Logs/n65/verdict-a4.md): a seed EXERCISES the refusal when its refused run refused at least one press or
        // its allowed run used the verb at least once; a seed that did neither compares two runs of the same play (a4's seed 1: 0 parries,
        // 0 presses refused, won by 11.9 s) and does not count toward GREEN
        var exercised = new List<bool>();
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            V65Clear x = V65ClearRun(path, roomId, seed, "none", cap, verbAware), y = V65ClearRun(path, roomId, seed, refuse, cap);
            on.Add(x); off.Add(y);
            bool ex = y.refused > 0 || UsesOf(x) > 0;
            exercised.Add(ex);
            Console.WriteLine($"  seed {seed}: {(verbAware ? "verb-aware" : "verb allowed")}: cleared {(x.cleared ? "yes" : "NO")} in {V65F(x.clearS)} s, deaths {x.deaths}, falls {x.falls}, {uses(x)}, STYLE {Style(x)}{(verbAware ? $", trigger held {x.trigger} steps" : "")} | {refuse} refused: cleared {(y.cleared ? "yes" : "NO")} in {V65F(y.clearS)} s, deaths {y.deaths}, falls {y.falls}, {uses(y)}, STYLE {Style(y)}, presses refused {y.refused}"
                + (ex ? " | exercised" : " | WARN not exercised: 0 presses refused and the verb used 0 times when allowed (the two runs play alike; not counted)"));
        }
        double med(List<V65Clear> l) { var v = l.Where(c => c.cleared).Select(c => c.clearS).OrderBy(q => q).ToList(); return v.Count == 0 ? double.NaN : v[v.Count / 2]; }
        var onX = on.Where((c, i) => exercised[i]).ToList(); var offX = off.Where((c, i) => exercised[i]).ToList();
        int nEx = onX.Count, need = (int)Math.Ceiling(0.6 * seeds); // 3 of 5
        int usesOn = on.Sum(UsesOf), refusedOff = off.Sum(c => c.refused);
        int clearedOff = offX.Count(c => c.cleared), clearedOn = onX.Count(c => c.cleared);
        double mOn = med(onX), mOff = med(offX);
        // GREEN = the verb could not be skipped (by this pilot), on the exercised seeds only: with it refused the room clears on fewer of them,
        // or slower by 25 % or more, or with more deaths; UNSURE when fewer than 60 % of the seeds (3 of 5) exercised the refusal
        bool slower = !double.IsNaN(mOn) && !double.IsNaN(mOff) && mOff >= mOn * 1.25;
        bool fewer = clearedOff < clearedOn, deadlier = offX.Sum(c => c.deaths) > onX.Sum(c => c.deaths);
        bool green = fewer || slower || deadlier;
        bool unsure = nEx < need;
        string verdict = unsure ? "UNSURE" : green ? "GREEN" : "RED";
        double dull(List<V65Clear> l) { long f = l.Sum(c => c.fight), u = l.Sum(c => c.aboveDull); return f > 0 ? 100.0 * u / f : 0; }
        Console.WriteLine($"n65verb skip: {verdict} | room {roomId}, verb65 {(verb65 ?? "none")}, refuse {refuse} | seeds exercising the refusal {nEx}/{seeds} (need {need}; presses refused {refusedOff}, the verb used {usesOn} time(s) when allowed)"
            + $" | on those: {(verbAware ? "verb-aware" : "verb allowed")}: cleared {clearedOn}/{nEx}, median {V65F(mOn)} s, deaths {onX.Sum(c => c.deaths)}, STYLE above DULL {dull(onX).ToString("0.0", V65Ci)} %{(verbAware ? $", trigger held {onX.Sum(c => c.trigger)} steps" : "")} | refused: cleared {clearedOff}/{nEx}, median {V65F(mOff)} s, deaths {offX.Sum(c => c.deaths)}, STYLE above DULL {dull(offX).ToString("0.0", V65Ci)} %"
            + $" | {(unsure ? $"too few seeds exercised the refusal ({nEx} < {need}): the comparison does not test the verb" : green ? "the verb is hard to skip for this pilot (" + string.Join(", ", new[] { fewer ? "fewer clears" : null, slower ? "slower by >= 25 %" : null, deadlier ? "more deaths" : null }.Where(q => q != null)) + ")" : "the room clears as well without the verb: it can be skipped (by this pilot)")}"
            + (usesOn == 0 ? " | NOTE the pilot never used the verb even when allowed: the comparison measures the room, not the verb" : "")
            + (refusedOff == 0 ? " | WARN no press was refused on any seed: the refusal never fired" : "")
            + (on.Sum(c => c.verbRefused) > 0 ? $" | REFUSED: verb65 '{verb65}' is not a verb the verb-aware pilot performs (the plain pilot ran)" : ""));
        return unsure ? 3 : green ? 0 : 1;
    }
}
