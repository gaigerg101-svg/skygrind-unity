// NIGHT41 builder P3 round 2 (the lead's ask): `n41pad real [--seeds 3] [--cap 90]`: aim41 against the REAL boss bodies' weak points (B8's fills:
// THE LOOM's spinneret in the window / the lit knee nearest you, Game/SurvivalSim.N41.Loom.cs; THE ICEBREAKER's stuck fist after the haymaker).
//   1. RECORD: the real fight in the sim with aim41 on (THE LOOM: n40loom stay's setup, loom40 on, Loom37Pilot; THE ICEBREAKER: n40brawl's setup,
//      Brawl40Pilot), every step: the player's eye, the body's centre (Enemy.AimPoint) and its weakPoint41 (active, at).
//   2. REPLAY into n41pad aim's Core rig (AimAssist, the pad's "assist", a finite breakaway cap) with n41pad aim's leading pad pilot: he aims at the
//      weak point while it is active (else the centre), 4 shots a second; a hit = the look ray within 0.45 m of the weak point WHILE active.
//      Hits per weak-point second, AimAssist.weakPoint41 off vs on. The bots' own fights are only the recording (their aim is not measured).
// Without B8's fills (a tree before fdbaa208) no weak point is ever active: "no weak-point steps" (nothing to measure).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class PadTrack41
    {
        public readonly List<Vec3> eye = new List<Vec3>(), centre = new List<Vec3>(), weak = new List<Vec3>();
        public readonly List<bool> active = new List<bool>();
        public void Add(SurvivalSim sim, Enemy body)
        {
            var (e, _) = sim.Aim();
            eye.Add(e);
            centre.Add(body.AimPoint);
            active.Add(body.weakPoint41.active);
            weak.Add(body.weakPoint41.at);
        }
    }

    /// <summary>Round 3: the replay pilot's thumb follows the aim line's own turn (feed-forward) before its 5/s correction; --feed k scales it (0 = --nofeed = round 2's pilot; default 1).</summary>
    static double PadFeed41 = 1;

    static int N41PadReal(string[] args)
    {
        int seeds = 3;
        double cap = 90;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1]);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], System.Globalization.CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        int fails = 0;
        int fi = Array.IndexOf(args, "--feed");
        PadFeed41 = Array.IndexOf(args, "--nofeed") >= 0 ? 0 : fi > 0 && fi < args.Length - 1 ? double.Parse(args[fi + 1], System.Globalization.CultureInfo.InvariantCulture) : 1;
        int only = Array.IndexOf(args, "--boss");
        string[] bosses = only > 0 && only < args.Length - 1 ? new[] { args[only + 1] } : new[] { "loom", "brawl" };
        foreach (string boss in bosses)
        {
            double offH = 0, onH = 0, act = 0;
            for (int s = 1; s <= seeds; s++)
            {
                PadTrack41 tr = boss == "loom" ? PadRecordLoom((uint)s, cap) : PadRecordBrawl((uint)s, cap);
                int a = 0;
                foreach (bool b in tr.active) if (b) a++;
                act += a * cfg.Dt;
                offH += PadTrackRun(cfg, tr, (uint)s, false);
                onH += PadTrackRun(cfg, tr, (uint)s, true);
            }
            if (act <= 0) { Console.WriteLine($"n41pad real {boss,-5}: no weak-point steps (B8's fills absent?)"); fails++; continue; }
            double off = offH / act, on = onH / act;
            bool ok = on > off;
            if (!ok) fails++;
            Console.WriteLine($"n41pad real {boss,-5}: weak point live {act:0.0} s over {seeds} fights; hits per live second off {off:0.000}  on {on:0.000}  {(ok ? "PASS" : "FAIL")}");
        }
        return fails;
    }

    static PadTrack41 PadRecordLoom(uint seed, double cap)
    {
        var tr = new PadTrack41();
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Loom40Level(true))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n40Override = new N40Blocks { loom40 = true };
        sim.n41Override = new N41Blocks { aim41 = true };
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return tr;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!sim.BossRoomBegin("loom37", out _, out _) || sim.loom37 == null) return tr;
        Loom37 L = sim.loom37;
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot();
        int capSteps = (int)(cap * 120);
        for (int st = 0; st < capSteps && L.Alive; st++)
        {
            if (sim.health.hp <= 0 || sim.flow != Skygrind.Game.Flow.Playing)
            {
                sim.health.hp = sim.health.max;
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
                if (sim.flow != Skygrind.Game.Flow.Playing) break;
            }
            pilot.Step(sim, st);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            tr.Add(sim, L.body);
        }
        return tr;
    }

    static PadTrack41 PadRecordBrawl(uint seed, double cap)
    {
        var tr = new PadTrack41();
        GameConfig cfg = Config();
        SurvivalSim.Brawl40Dir = MapGenRepo;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n40Override = new N40Blocks { brawl40 = true };
        sim.n41Override = new N41Blocks { aim41 = true };
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.Brawl40Place(Brawl40At)) return tr;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        Vec3 spawn = default;
        double yaw = 0, yawL = 0;
        if (!sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw)) return tr;
        Brawl40 g = sim.brawl40;
        var si = (ScriptedInput)sim.input;
        var pilot = new Brawl40Pilot { hand = new Mulberry32(seed * 7919u + 40u) };
        int capSteps = (int)(cap * 120) + MovementMath.StepsFor(g.p.revealSeconds, cfg.Dt);
        for (int st = 0; st < capSteps && g.Alive; st++)
        {
            pilot.Step(sim, yawL, st);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            tr.Add(sim, g.body);
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw);
                sim.Respawn(spawn, yaw);
            }
        }
        return tr;
    }

    /// <summary>Replay a recorded track into the Core rig: hits on the weak point while it is active (a count; the caller divides by live seconds).</summary>
    static int PadTrackRun(GameConfig cfg, PadTrack41 tr, uint seed, bool aim41)
    {
        double dt = cfg.Dt, DEG = Math.PI / 180;
        var events = new EventBus();
        var enemies = new EnemyRegistry();
        var world = new TriangleWorld();
        world.AddBox(new Vec3(-1e4, -1e3 - 1, -1e4), new Vec3(1e4, -1e3, 1e4)); // far under every arena: the rig's sight is open (the fight's own sight was the sim's)
        int n = tr.eye.Count;
        if (n == 0) return 0;
        Vec3 eye = tr.eye[0];
        double yaw = 0, pitch = 0;
        double cap = cfg.Num("gamepad.yawRate") * cfg.Num("gamepad.breakawayShare");
        var assist = new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "revolver", () => false, () => "assist",
            () => cfg.Num("settings.snapStrength"), () => cap) { weakPoint41 = aim41, weakReach41 = PadReach41, weakAcquire41 = PadAcq41 };
        var boss = new PadBoss41 { kind = "padBoss41", id = 1, alive = true, hp = 1e9, maxHp = 1e9 };
        enemies.all.Add(boss);
        var rng = new Mulberry32(seed * 7919 + 41);
        int lag = MovementMath.StepsFor(0.2, dt), span = MovementMath.StepsFor(0.1, dt), every = MovementMath.StepsFor(0.25, dt);
        double turn = cfg.Num("gamepad.yawRate") * DEG, leadT = 0.2 + 1.0 / 5, jy = 0, jp = 0;
        Vec3 Want(int k) => tr.active[k] ? tr.weak[k] : tr.centre[k];
        (yaw, pitch) = AssistMath.YawPitchTo(eye, tr.centre[0]);
        int hits = 0, liveSteps = 0, lockedLive = 0, onWeakLive = 0;
        double prevTy = 0, prevTp = 0;
        bool hasPrevT = false;
        List<double> dbgAng = Environment.GetEnvironmentVariable("N41PAD_DEBUG") == "1" ? new List<double>() : null, dbgDist = dbgAng != null ? new List<double>() : null;
        for (int k = 0; k < n; k++)
        {
            eye = tr.eye[k];
            Vec3 c = tr.centre[k];
            boss.position = new Vec3(c.x, c.y - 3, c.z);
            if (tr.active[k]) boss.weakPoint41.Set(tr.weak[k]); else boss.weakPoint41.Clear();
            int s0 = Math.Max(0, k - lag), s1 = Math.Max(0, k - lag - span);
            Vec3 seen = Want(s0), older = Want(s1);
            if (tr.active[s0] == tr.active[s1])
            {
                double f = leadT / (span * dt);
                seen = new Vec3(seen.x + (seen.x - older.x) * f, seen.y + (seen.y - older.y) * f, seen.z + (seen.z - older.z) * f);
            }
            if (k % 12 == 0) { jy = (rng.Next() * 2 - 1) * DEG; jp = (rng.Next() * 2 - 1) * DEG; }
            var (ty, tp) = AssistMath.YawPitchTo(eye, seen);
            double dy = Math.Atan2(Math.Sin(ty + jy - yaw), Math.Cos(ty + jy - yaw)), dp = tp + jp - pitch;
            dy *= 5 * dt; dp *= 5 * dt;
            if (PadFeed41 > 0 && hasPrevT) { dy += PadFeed41 * Math.Atan2(Math.Sin(ty - prevTy), Math.Cos(ty - prevTy)); dp += PadFeed41 * (tp - prevTp); } // round 3: the thumb follows the line's turn (feed-forward: his own motion and the led target), then corrects 5 x the error per second
            prevTy = ty; prevTp = tp; hasPrevT = true;
            double mag = Math.Sqrt(dy * dy + dp * dp), most = turn * dt;
            if (mag > most) { dy *= most / mag; dp *= most / mag; }
            if (PadFriction41) { double ls = assist.LookScale; dy *= ls; dp *= ls; } // round 3: the game scales the stick by the lock's friction (SurvivalGame.cs:283, input.sensMult = ... x assist.LookScale)
            yaw += dy;
            pitch += dp;
            bool shoot = k % every == 0;
            var frame = new CombatInput { weapon = -1, fire = new Button { held = shoot, pressed = shoot } };
            assist.Step(dt, ref frame, ref yaw, ref pitch);
            if (tr.active[k]) { liveSteps++; if (dbgAng != null) { dbgAng.Add(AssistMath.AngleTo(eye, AssistMath.LookDir(yaw, pitch), tr.weak[k]) / DEG); dbgDist.Add(Vec3.DistanceTo(eye, tr.weak[k])); } if (assist.Target == boss) lockedLive++; if (AssistMath.AngleTo(eye, AssistMath.LookDir(yaw, pitch), tr.weak[k]) < Math.Atan2(0.45, Math.Max(0.5, Vec3.DistanceTo(eye, tr.weak[k])))) onWeakLive++; }
            if (frame.fire.pressed && tr.active[k])
            {
                Vec3 w = tr.weak[k], d = AssistMath.LookDir(yaw, pitch);
                double bx = w.x - eye.x, by = w.y - eye.y, bz = w.z - eye.z;
                double tt = bx * d.x + by * d.y + bz * d.z;
                double qx = eye.x + d.x * tt - w.x, qy = eye.y + d.y * tt - w.y, qz = eye.z + d.z * tt - w.z;
                if (tt > 0 && qx * qx + qy * qy + qz * qz <= 0.45 * 0.45) hits++;
            }
        }
        if (Environment.GetEnvironmentVariable("N41PAD_DEBUG") == "1") Console.WriteLine($"  seed {seed} aim41 {aim41}: hits {hits} live {liveSteps} lockedLive {lockedLive} onWeakLive {onWeakLive} locks {assist.locks} releases [{string.Join(",", System.Linq.Enumerable.Select(assist.releases, kv => kv.Key + "=" + kv.Value))}]");
        if (dbgAng != null && dbgAng.Count > 0) { dbgAng.Sort(); dbgDist.Sort(); Console.WriteLine($"    look-to-weak deg p25 {dbgAng[dbgAng.Count / 4]:0.0} p50 {dbgAng[dbgAng.Count / 2]:0.0} p75 {dbgAng[dbgAng.Count * 3 / 4]:0.0}; distance m p25 {dbgDist[dbgDist.Count / 4]:0.0} p50 {dbgDist[dbgDist.Count / 2]:0.0} p75 {dbgDist[dbgDist.Count * 3 / 4]:0.0}"); }
        return hits;
    }
}
