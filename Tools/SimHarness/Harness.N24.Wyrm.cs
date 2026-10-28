// NIGHT24 builder B: `wyrm [--tag t] [--seed N] [--cap M] [--selftest] [--noprint]` the KILN WYRM's headless fight (Core/Enemies/Wyrm.cs), standalone:
// the worm in its own arena (Design/maps/wyrm-arena.json) with a bot player (the real Mover on the arena's boxes, the real Health, the
// revolver's damage and fire interval from config) and the real BossKit. Prints one line per check and the contract line; exit 1 on a FAIL.
//   table     every mark's tell and the spit / core tells are at least stepsFor(0.5 s); the config's tells read from design.n24.wyrm.tells
//   spawn     over 10000 sampled steps (three fights: from the start, from phase 2, from phase 3) no piece comes within 6 m of the player spawn
//   solids    the head never sits inside a pillar or a platform while it is above ground (the room's own boxes)
//   fight     the bot kills it at a fixed seed within 6 minutes of game time (the bot's time is printed; the human target is 2.5 to 3.5 min);
//             the attack log: every tell >= 0.5 s ahead of its attack, no two attacks resolve inside 0.5 s, the web's boss events were all emitted
//   tape      the bot's inputs replayed into a fresh fight: the state hash every second is EQUAL (step exact)
//   fp        the composed SURVIVAL fingerprint with every NIGHT24 block off is EQUAL to the baseline (the worm changes nothing with its row off)
// The bot sees the worm's future (Wyrm.PredictCenter): a competent player who has learnt the cycle, not a reader of the code; its aim turns at
// a limited rate and it fires only when its ray finds a piece. A contract line "wyrm PASS: killed at MM:SS ..." is what the lead copies.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class WyrmRig
    {
        public readonly GameConfig cfg;
        public readonly WyrmArena arena;
        public readonly ICollisionWorld world;
        public readonly EventBus events;
        /// <summary>The composed sim (null: the standalone rig) and the room's origin in the world.</summary>
        public readonly SurvivalSim sim;
        public readonly Vec3 origin;
        public readonly Health health;
        public readonly Mover mover;
        public readonly BossKit kit;
        public readonly Wyrm wyrm;
        public readonly double dt;
        public int step;
        public readonly List<(int step, string phase)> phases = new List<(int, string)>();
        public readonly double revolverDamage;
        public readonly int revolverInterval;
        public int fireCool, deaths;
        public int shotsFired, shotsHit, pushes;
        public Vec3 lastPush;

        /// <summary>The rig over the COMPOSED sim: its player (Mover, Health, the real weapons through the sim's input), its worm and BossKit.</summary>
        public WyrmRig(SurvivalSim sim)
        {
            this.sim = sim;
            cfg = sim.cfg;
            arena = sim.wyrm24Arena;
            origin = sim.wyrm24Origin;
            dt = cfg.Dt;
            world = sim.world;
            events = sim.events;
            health = sim.health;
            mover = sim.mover;
            kit = sim.bossKit;
            wyrm = sim.wyrm24;
            events.On<BossEvent>("bossPhase", e => phases.Add((step, e.phase)));
        }

        public WyrmRig(GameConfig cfg, Dictionary<string, object> bossJson, WyrmArena arena, uint seed, double playerHp, bool shed = false)
        {
            events = new EventBus();
            this.cfg = cfg;
            this.arena = arena;
            dt = cfg.Dt;
            world = arena.BuildWorld(Vec3.Zero);
            health = new Health(playerHp, events, "player");
            mover = new Mover(cfg.movement, world);
            mover.Reset(arena.spawn.x, arena.spawn.y, arena.spawn.z);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => { pushes++; lastPush = new Vec3(x, y, z); mover.Impulse(x, y, z); } };
            kit = new BossKit(cfg, events, null, target);
            int ids = 0;
            WyrmParams wp = WyrmParams.From(cfg, bossJson);
            if (shed) wp.tailShed = true;
            wyrm = new Wyrm(cfg, wp, events, world, target, () => ++ids);
            wyrm.playerBody = () => mover.Body;
            kit.bosses["wyrm"] = wyrm;
            wyrm.NewRun(seed);
            revolverDamage = cfg.Num("weapons.revolver.damage");
            revolverInterval = MovementMath.StepsFor(cfg.Num("weapons.revolver.fireInterval"), dt);
            events.On<BossEvent>("bossPhase", e => phases.Add((step, e.phase)));
            kit.Start("wyrm");
        }

        public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + 1.6, mover.position.z);

        public static Vec3 AimDir(double yaw, double pitch)
        {
            Vec3 f = MovementMath.WishDirection(0, 1, yaw);
            double cp = JsMath.Cos(pitch);
            return new Vec3(f.x * cp, JsMath.Sin(pitch), f.z * cp);
        }

        /// <summary>Is the line from the eye to a point open (the room's walls and pillars)?</summary>
        public bool Open(Vec3 to)
        {
            Vec3 e = Eye, d = to - e;
            double len = d.Length;
            if (len < 1e-6) return true;
            return !(world.RayIntersect(e, d / len, out RayHit hit) && hit.distance < len - 0.5);
        }

        public bool Shoot(double yaw, double pitch)
        {
            Vec3 d = AimDir(yaw, pitch);
            double t = wyrm.Raycast(Eye, d);
            if (t < 0) return false;
            if (world.RayIntersect(Eye, d, out RayHit hit) && hit.distance < t) return false;
            return wyrm.TakeHit(Eye, d, revolverDamage, "revolver", out double dist) >= 0;
        }

        public void Step(in InputFrame f, bool fire)
        {
            if (sim != null)
            {
                var si = (ScriptedInput)sim.input;
                si.SetLook(f.yaw, f.pitch);
                si.Set(f, fire ? new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } } : new CombatInput { weapon = -1 });
                if (fire) shotsFired++;
                sim.Step();
                step++;
                if (sim.flow == Skygrind.Game.Flow.Down || health.Dead)
                {
                    deaths++;
                    sim.downSteps = 0;
                    sim.flow = Skygrind.Game.Flow.Playing;
                    health.Reset();
                    sim.Respawn(sim.Wyrm24Spawn, 0);
                }
                return;
            }
            mover.Step(f, dt);
            if (fireCool > 0) fireCool--;
            if (fire && fireCool == 0)
            {
                fireCool = revolverInterval;
                shotsFired++;
                if (Shoot(f.yaw, f.pitch)) shotsHit++;
            }
            wyrm.Step(dt);
            kit.Step(dt);
            step++;
            if (health.Dead)
            {
                deaths++;
                health.Reset();
                mover.Reset(arena.spawn.x, arena.spawn.y, arena.spawn.z);
            }
        }

        public ulong Hash()
        {
            ulong h = 1469598103934665603UL;
            void Mix(double v)
            {
                h ^= (ulong)BitConverter.DoubleToInt64Bits(v);
                h *= 1099511628211UL;
            }
            Mix(wyrm.body.hp);
            Mix(wyrm.HeadPosition.x);
            Mix(wyrm.HeadPosition.y);
            Mix(wyrm.HeadPosition.z);
            Mix(health.hp);
            Mix(mover.position.x);
            Mix(mover.position.y);
            Mix(mover.position.z);
            Mix(wyrm.RngState);
            Mix(wyrm.shots.Count);
            Mix(wyrm.boxes[wyrm.PieceCount / 2].c.x);
            Mix(wyrm.boxes[wyrm.PieceCount - 1].c.z);
            foreach (char ch in wyrm.phase) Mix(ch);
            return h;
        }
    }

    /// <summary>The competent-player bot: reads the worm's predicted pieces, moves to the safest reachable spot near a standoff, aims
    /// at the head (or the nearest open piece) at a limited turn rate and fires only on a hit.</summary>
    sealed class WyrmBot
    {
        readonly bool human;
        readonly Queue<Vec3[]> hist = new Queue<Vec3[]>();
        double dtv = 1.0 / 120;
        const int LagSteps = 30; // the human sees the worm 0.25 s late
        public WyrmBot(bool human = false) { this.human = human; }

        /// <summary>Where piece i will be in tau s: the bot with the future (PredictCenter), or a human's constant-velocity guess from what it sees now.</summary>
        Vec3 Pred(Wyrm w, int i, double tau)
        {
            if (!human) return w.PredictCenter(i, tau);
            Vec3 c = w.boxes[i].c;
            Vec3 v = (c - w.prevCenters[i]) * (1 / dtv);
            return c + v * tau;
        }

        double yaw, pitch;
        Vec3 goal;
        int goalAge, reacquire;
        double aimErr = 9;
        bool haveGoal;
        const double AimRate = 7.0; // rad/s, a flick
        const double Reaction = 0.18; // s before the first shot at a newly seen target
        public int pieceTarget = -1;

        public InputFrame Next(WyrmRig r, out bool fire)
        {
            Wyrm w = r.wyrm;
            dtv = r.dt;
            Vec3 me = r.mover.position;
            if (human)
            {
                var snap = new Vec3[w.PieceCount];
                for (int i = 0; i < snap.Length; i++) snap[i] = w.boxes[i].c;
                hist.Enqueue(snap);
                while (hist.Count > LagSteps + 1) hist.Dequeue();
            }
            // --- where to stand
            if (!haveGoal || goalAge++ >= (human ? 15 : 6))
            {
                goal = PickGoal(r);
                haveGoal = true;
                goalAge = 0;
            }
            // --- what to shoot
            Vec3 eye = r.Eye;
            int pick = -1;
            double bestD = 1e9;
            for (int i = 0; i < w.PieceCount; i++)
            {
                if (w.buried[i]) continue;
                Vec3 c = w.boxes[i].c;
                if (!r.Open(c)) continue;
                double d = Vec3.DistanceTo(eye, c) * (i == 0 ? (human ? 0.15 : 0.35) : 1);
                if (w.ShedActive && i >= w.ShedFirst && i < w.ShedEnd) d = 0.01 * (1 + i - w.ShedFirst);
                if (d < bestD)
                {
                    bestD = d;
                    pick = i;
                }
            }
            if (pick != pieceTarget)
            {
                pieceTarget = pick;
                reacquire = MovementMath.StepsFor(human ? 0.3 : Reaction, r.dt);
            }
            if (pick >= 0)
            {
                Vec3 at = w.boxes[pick].c;
                if (human) at = hist.Peek()[pick]; // aims where it saw the piece 0.25 s ago
                if (pick == 0 && w.CoreOpen) at = at + w.boxes[0].u * w.p.coreUp - w.boxes[0].f * w.p.coreBack;
                if (!human)
                {
                    Vec3 lead = w.PredictCenter(pick, 0.06);
                    at = at + (lead - w.boxes[pick].c);
                }
                Vec3 d = at - eye;
                double wantYaw = JsMath.Atan2(-d.x, -d.z), wantPitch = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                double step = (human ? 4.5 : AimRate) * r.dt;
                aimErr = Math.Max(Math.Abs(Wrap(wantYaw - yaw)), Math.Abs(wantPitch - pitch));
                double dy = Wrap(wantYaw - yaw), dp = wantPitch - pitch;
                yaw += Math.Max(-step, Math.Min(step, dy));
                pitch += Math.Max(-step, Math.Min(step, dp));
            }
            if (reacquire > 0) reacquire--;
            fire = human ? pick >= 0 && reacquire == 0 && aimErr < 0.035 && r.fireCool == 0 : pick >= 0 && reacquire == 0 && r.wyrm.Raycast(eye, WyrmRig.AimDir(yaw, pitch)) >= 0 && r.fireCool == 0;
            // --- move
            Vec3 to = goal - me;
            double dist = JsMath.Hypot(to.x, to.z);
            var f = new InputFrame { yaw = yaw, pitch = pitch };
            if (dist > 0.6)
            {
                Vec3 want = new Vec3(to.x / dist, 0, to.z / dist);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yaw), right = MovementMath.WishDirection(1, 0, yaw);
                f.moveZ = want.x * fwd.x + want.z * fwd.z;
                f.moveX = want.x * right.x + want.z * right.z;
            }
            return f;
        }

        static double Wrap(double a)
        {
            while (a > Math.PI) a -= 2 * Math.PI;
            while (a < -Math.PI) a += 2 * Math.PI;
            return a;
        }

        Vec3 PickGoal(WyrmRig r)
        {
            Wyrm w = r.wyrm;
            Vec3 me = r.mover.position;
            Vec3 head = w.HeadPosition;
            double best = -1e18;
            Vec3 bestC = me;
            double walk = 9.0;
            for (int k = -1; k < 48; k++)
            {
                Vec3 c;
                if (k < 0) c = me;
                else
                {
                    double ang = (k % 16) * Math.PI / 8, rad = k < 16 ? 3 : k < 32 ? 6 : 10;
                    c = new Vec3(me.x + JsMath.Cos(ang) * rad, me.y, me.z + JsMath.Sin(ang) * rad);
                }
                Vec3 lc = new Vec3(c.x - r.origin.x, c.y - r.origin.y, c.z - r.origin.z);
                if (Math.Abs(lc.x) > 27 || Math.Abs(lc.z) > 27) continue;
                if (r.arena.InSolid(new Vec3(lc.x, lc.y + 1, lc.z), 1.8, "pillar")) continue;
                double d = Vec3.DistanceTo(me, c);
                double tArr = d / walk;
                double clear = 12;
                for (int ti = 0; ti <= 10; ti++)
                {
                    double tau = ti * 0.15;
                    double m = d < 1e-6 ? 0 : Math.Min(1, walk * tau / d);
                    Vec3 pos = new Vec3(me.x + (c.x - me.x) * m, me.y + 0.9, me.z + (c.z - me.z) * m);
                    for (int i = 0; i < w.PieceCount; i += (i < 2 ? 1 : 2))
                    {
                        Vec3 pc = Pred(w, i, tau);
                        if (pc.y < -1) continue;
                        double rad = i == 0 ? 2.4 : 1.7;
                        double cl = Vec3.DistanceTo(pos, pc) - rad - 0.9;
                        if (cl < clear) clear = cl;
                    }
                }
                double standoff = Math.Abs(Vec3.DistanceTo(new Vec3(c.x, 0, c.z), new Vec3(head.x, 0, head.z)) - 16);
                double score = Math.Min(clear, 6) * 4 - 0.12 * d - 0.05 * standoff + (k < 0 ? 0.6 : 0);
                if (clear < 0) score -= 100;
                if (score > best)
                {
                    best = score;
                    bestC = c;
                }
            }
            return bestC;
        }
    }

    static string WyrmMmSs(int steps, double dt)
    {
        double sec = steps * dt;
        return $"{(int)(sec / 60)}:{sec % 60:00.0}";
    }

    static partial void WyrmCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            Type t = Type.GetType("Skygrind.Core.Tests.WyrmTests");
            if (t == null)
            {
                Console.WriteLine("wyrm --selftest: compile Assets/Tests/EditMode/WyrmTests.cs in (-p:Extra=../../Assets/Tests/EditMode/WyrmTests.cs)");
                code = 1;
                return;
            }
            int failed = NUnit.Framework.MiniRunner.Run(t, Console.WriteLine);
            Console.WriteLine($"wyrm --selftest {(failed == 0 ? "PASS" : "FAIL")}");
            code = failed == 0 ? 0 : 1;
            return;
        }
        uint seed = 7;
        if (Array.IndexOf(args, "--composed") >= 0)
        {
            WyrmComposed(ref code);
            return;
        }
        if (Array.IndexOf(args, "--contact") >= 0)
        {
            WyrmContact(ref code);
            return;
        }
        if (Array.IndexOf(args, "--shed") >= 0)
        {
            WyrmShedLine(ref code);
            return;
        }
        if (Array.IndexOf(args, "--human") >= 0)
        {
            WyrmHuman(args, ref code);
            return;
        }
        double capMin = 6;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seed") seed = uint.Parse(args[i + 1]);
            if (args[i] == "--cap") capMin = double.Parse(args[i + 1]);
        }
        GameConfig cfg = Config();
        string repo = Path.GetFullPath(Path.Combine(Root, ".."));
        var boss = Wyrm.LoadBoss(Path.Combine(repo, "Design", "bosses", "wyrm.json"));
        WyrmArena arena = WyrmArena.Load(Path.Combine(repo, "Design", "maps", "wyrm-arena.json"));
        WyrmParams prm = WyrmParams.From(cfg, boss);
        double dt = cfg.Dt;
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Console.WriteLine($"wyrm: row design.n24.wyrm.enabled = {cfg.Design("n24.wyrm.enabled")}; the fight is explicit (the row only switches layer 1's boss room); seed {seed}");

        // --- table: the tells ------------------------------------------------------------------------------------------------
        int minTell = MovementMath.StepsFor(0.5, dt);
        var rows = new List<string>();
        foreach (var kv in prm.tells)
        {
            if (kv.Key == "resolveGap") continue;
            rows.Add($"{kv.Key} {kv.Value:0.0#}s");
            Need(MovementMath.StepsFor(kv.Value, dt) >= minTell, $"tell '{kv.Key}' is {kv.Value} s, under 0.5 s");
        }
        var patternsSeen = new List<WyrmPattern>(prm.patterns.Values) { prm.emerge };
        foreach (WyrmPattern pt in patternsSeen)
            foreach (WyrmVariant v in pt.variants)
                foreach (WyrmMarkDef m in v.marks)
                    Need(prm.tells.ContainsKey(m.tell) && MovementMath.StepsFor(prm.tells[m.tell], dt) >= minTell, $"mark '{m.name}' of {pt.name} has no tell of 0.5 s or more");
        Need(MovementMath.StepsFor(prm.resolveGap, dt) >= minTell, "resolveGap is under 0.5 s");
        Console.WriteLine($"  table: tells {string.Join(", ", rows)}; resolveGap {prm.resolveGap}s; cycle {string.Join(" > ", prm.cycle)}; min {minTell} steps");

        // --- spawn clearance and solids: a passive player at the spawn, three fights of 10000 steps -----------------------------------
        Vec3 spawnC = new Vec3(arena.spawn.x, arena.spawn.y + 0.9, arena.spawn.z);
        double minSpawn = 1e9;
        int solidHeads = 0, solidBody = 0, sampled = 0, aboveGround = 0;
        string[] flav = { "p1", "p2", "p3" };
        var contactSeen = new Dictionary<string, int>();
        var solidWhere = new Dictionary<string, int>();
        var solidFirst = new Dictionary<string, Vec3>();
        for (int fi = 0; fi < 3; fi++)
        {
            var rig = new WyrmRig(cfg, boss, arena, seed + (uint)fi * 101u, 1e9);
            var idle = new InputFrame { yaw = 0, pitch = 0 };
            for (int st = 0; st < 10000; st++)
            {
                if (fi > 0 && rig.wyrm.phase == "p1" && st > 1) ForcePhase(rig, flav[fi], idle);
                rig.Step(idle, false);
                sampled++;
                Wyrm w = rig.wyrm;
                for (int i = 0; i < w.PieceCount; i++)
                {
                    if (w.buried[i]) continue;
                    double d = Vec3.DistanceTo(spawnC, w.boxes[i].c);
                    if (d < minSpawn) minSpawn = d;
                    if (i % 4 == 0 && (arena.InSolid(w.boxes[i].c, 0, "pillar") || arena.InSolid(w.boxes[i].c, 0, "platform")))
                    {
                        if (i == 0) solidHeads++;
                        else solidBody++;
                        string key = (i == 0 ? "head in " : "body in ") + w.PatternName + " side " + w.Side;
                        solidWhere[key] = solidWhere.TryGetValue(key, out int sc) ? sc + 1 : 1;
                        if (!solidFirst.ContainsKey(key)) solidFirst[key] = w.boxes[i].c;
                    }
                }
                if (!w.buried[0]) aboveGround++;
            }
            foreach (var kv in rig.wyrm.stats) contactSeen[kv.Key] = (contactSeen.TryGetValue(kv.Key, out int n) ? n : 0) + kv.Value;
        }
        Console.WriteLine($"  spawn: over {sampled} sampled steps (3 fights: p1, p2, p3) the nearest piece centre to the spawn point was {minSpawn:0.0} m (need >= {prm.spawnClear}); head above ground {aboveGround} steps; head in a pillar or platform {solidHeads} steps, body samples in one {solidBody}");
        foreach (var kv in solidWhere) Console.WriteLine($"    solid: {kv.Key}: {kv.Value} samples, first at ({solidFirst[kv.Key].x:0.0}, {solidFirst[kv.Key].y:0.0}, {solidFirst[kv.Key].z:0.0})");
        Need(minSpawn >= prm.spawnClear, $"a piece came within {minSpawn:0.0} m of the spawn");
        Need(solidHeads == 0, $"the head sat in a pillar or platform {solidHeads} steps");

        // --- fight: the bot ----------------------------------------------------------------------------------------------------
        var main = new WyrmRig(cfg, boss, arena, seed, cfg.Num("health.max"));
        var bot = new WyrmBot();
        var tape = new List<(InputFrame f, bool fire)>();
        var hashes = new List<ulong>();
        int cap = (int)(capMin * 60 / dt);
        int hit0 = 0;
        var wall = System.Diagnostics.Stopwatch.StartNew();
        while (main.wyrm.Alive && main.step < cap * 2)
        {
            InputFrame f = bot.Next(main, out bool fire);
            tape.Add((f, fire));
            main.Step(f, fire);
            if (main.step % 120 == 0) hashes.Add(main.Hash());
        }
        bool killed = !main.wyrm.Alive;
        int killStep = main.step;
        string phaseLine = string.Join(", ", main.phases.ConvertAll(x => $"{x.phase}@{WyrmMmSs(x.step, dt)}"));
        Wyrm ww = main.wyrm;
        Console.WriteLine($"  fight: {(killed ? "KILLED" : "NOT KILLED")} at {WyrmMmSs(killStep, dt)} game time ({killStep} steps, {wall.Elapsed.TotalSeconds:0.0} s wall); bot deaths {main.deaths}, player hp {main.health.hp:0}; shots {main.shotsHit}/{main.shotsFired} hit");
        Console.WriteLine($"  phases: {phaseLine}");
        var st2 = ww.stats;
        Console.WriteLine($"  stats: body hits {st2["bodyHits"]}, head hits {st2["headHits"]}, core hits {st2["coreHits"]}; contacts body {st2["contactsBody"]} head {st2["contactsHead"]} lunge {st2["contactsLunge"]}; spit hits {st2["shotsHit"]} of {st2["volleys"]} volleys, shards {st2["shards"]}; dives {st2["dives"]}, lunges {st2["lunges"]}, squeezes {st2["squeezes"]}, core opens {st2["coreOpens"]}");
        Need(killed, "the bot did not kill the worm");
        Need(killed && killStep * dt <= 360, $"the bot's kill took {WyrmMmSs(killStep, dt)}, over 6 minutes");

        // --- the attack log ----------------------------------------------------------------------------------------------------
        int gapSteps = MovementMath.StepsFor(prm.resolveGap, dt);
        var tellMap = new Dictionary<string, string> { { "lungeCommit", "lunge" }, { "shed", "shed" }, { "diveExit", "dive" }, { "squeeze", "squeeze" }, { "spit", "spit" } };
        int resolves = 0, minGap = int.MaxValue, badTell = 0, badGap = 0, noTell = 0;
        int lastRes = -1000000;
        string lastName = "";
        var lastTell = new Dictionary<string, int>();
        double diveMin = 1e9, diveMax = 0;
        foreach (WyrmLog l in ww.log)
        {
            if (!l.resolve)
            {
                if (MovementMath.StepsFor(l.seconds, dt) < minTell) badTell++;
                lastTell[l.name] = l.step;
                continue;
            }
            resolves++;
            if (lastRes > -1000000)
            {
                int g = l.step - lastRes;
                if (g < minGap) minGap = g;
                if (g < gapSteps) badGap++;
            }
            lastRes = l.step;
            lastName = l.name;
            if (l.name == "diveExit" && lastTell.TryGetValue("crack", out int cs))
            {
                double hid = (l.step - cs) * dt;
                diveMin = Math.Min(diveMin, hid);
                diveMax = Math.Max(diveMax, hid);
            }
            if (!lastTell.TryGetValue(tellMap[l.name], out int ts) || l.step - ts < minTell - 1) { noTell++; Console.WriteLine($"    no tell: {l.name} at step {l.step}, last tell {(lastTell.TryGetValue(tellMap[l.name], out int t2) ? t2 : -1)}, phase p{ww.phase}"); }
        }
        Console.WriteLine($"  attacks: {resolves} resolved, nearest two {(minGap == int.MaxValue ? 0 : minGap)} steps ({(minGap == int.MaxValue ? 0 : minGap * dt):0.00} s; need >= {gapSteps}); tells under 0.5 s {badTell}; attacks without a tell >= 0.5 s ahead {noTell}; dive: crack to eruption {diveMin:0.0} to {diveMax:0.0} s (the wall spot is marked {prm.tells["dive"]:0.0} s ahead)");
        Need(badTell == 0, "a tell under 0.5 s");
        Need(badGap == 0, $"{badGap} attacks resolved inside {prm.resolveGap} s of another");
        Need(noTell == 0, $"{noTell} attacks had no tell of 0.5 s ahead");
        string[] want = { "bossIntro", "bossBeat", "bossTell", "bossPhase", "bossHit", "bossPart", "bossDying", "bossDown" };
        var missing = new List<string>();
        foreach (string e in want)
            if (!main.events.Emitted.ContainsKey(e)) missing.Add(e);
        Console.WriteLine($"  events: {(missing.Count == 0 ? "all eight of the web's boss events emitted" : "MISSING " + string.Join(", ", missing))} (bossHit x{(main.events.Emitted.TryGetValue("bossHit", out int bh) ? bh : 0)}, bossTell x{(main.events.Emitted.TryGetValue("bossTell", out int bt) ? bt : 0)})");
        Need(missing.Count == 0, "boss events missing: " + string.Join(", ", missing));

        // --- tape: replay the bot's inputs into a fresh fight ------------------------------------------------------------------
        var rep = new WyrmRig(cfg, boss, arena, seed, cfg.Num("health.max"));
        int diff = -1;
        int rh = 0;
        for (int i = 0; i < tape.Count && rep.wyrm.Alive; i++)
        {
            rep.Step(tape[i].f, tape[i].fire);
            if (rep.step % 120 == 0)
            {
                if (rh >= hashes.Count || hashes[rh] != rep.Hash())
                {
                    diff = rep.step;
                    break;
                }
                rh++;
            }
        }
        bool equal = diff < 0 && rep.step == killStep && rep.wyrm.Alive == main.wyrm.Alive && rep.wyrm.body.hp == main.wyrm.body.hp;
        Console.WriteLine($"  tape: {tape.Count} steps of the bot's input replayed: {(equal ? "EQUAL" : "DIFFERS at step " + diff)} ({rh} second-hashes compared)");
        Need(equal, "the tape replay differs");

        // --- sim: the worm inside the composed SurvivalSim (SurvivalSim.Wyrm24.cs), the way layer 1 will start it -----------------------------
        {
            SurvivalSim.Wyrm24Dir = repo;
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            bool began = sim.Wyrm24Begin();
            Need(began, "Wyrm24Begin did not start the fight");
            if (began)
            {
                sim.Respawn(sim.Wyrm24Spawn, 0);
                Wyrm sw = sim.wyrm24;
                bool registered = sim.enemies.all.Contains(sw.body);
                int steps = 0;
                while (steps++ < 3600)
                {
                    sim.health.hp = sim.health.max;
                    sim.Step();
                }
                bool kitAlive = sim.bossKit.Alive;
                Vec3 head = sw.boxes[0].c;
                double hit = sw.Raycast(new Vec3(head.x, head.y + 15, head.z), new Vec3(0, -1, 0));
                double dealt = EnemyHits.Filter(sw.body, 25, true, "revolver", false, new Vec3(head.x, head.y + 15, head.z), sim.events, out bool blocked);
                Console.WriteLine($"  sim: began {began}, registered as an enemy {registered}, after {steps - 1} steps phase {sw.phase} pattern {sw.PatternName}, kit alive {kitAlive}, a shot at the head through EnemyHits.Filter lands {dealt} (blocked {blocked}), player at ({sim.mover.position.x:0}, {sim.mover.position.z:0}) in the room");
                Need(registered && sw.Alive && kitAlive, "the worm is not alive and registered inside the sim");
                Need(!blocked && Math.Abs(dealt - 50) < 1e-9, $"the registry's shot landed {dealt} (blocked {blocked}), not the head's 50");
            }
            // the snapshot: the NIGHT24 sections (fist, BIG PICK, worm stream) save and load to the same bytes
            SurvivalSim sa = SurvivalSim.Headless(cfg, Map());
            sa.noLab = sa.noRecords = true;
            sa.n24Override = new N24Blocks { wyrm = true, chargeFist = true, bigPick = true };
            sa.setup.mode = RunSetup.Survival;
            sa.StartRun(3, null, null);
            for (int i = 0; i < 600; i++) sa.Step();
            sa.run.DebugSkipTo(3);
            for (int g2 = 0; g2 < 120 * 60 && sa.run.state != SurvivalRun.State.Break; g2++)
            {
                sa.husks.KillAll();
                sa.health.hp = sa.health.max;
                sa.Step();
            }
            sa.fist24.charge = 0.5;
            string json = RunSnapshot.Save(sa.SnapshotSections());
            SurvivalSim sb = SurvivalSim.Headless(cfg, Map());
            sb.noLab = sb.noRecords = true;
            sb.n24Override = sa.n24Override;
            sb.setup.mode = RunSetup.Survival;
            sb.StartRun(3, null, null);
            RunSnapshot.Load(json, sb.SnapshotSections());
            bool same = RunSnapshot.Save(sb.SnapshotSections()) == json && sb.fist24.charge == 0.5 && json.Contains("fist24") && json.Contains("bigPick24");
            Console.WriteLine($"  snapshot: fist24 / bigPick24 sections in the run snapshot ({json.Length} chars), restore saves the same bytes and the charge 0.5 comes back: {same}");
            Need(same, "the NIGHT24 snapshot sections do not round-trip");
            SurvivalSim.Wyrm24Dir = null;
        }

        // --- fingerprint with the row off ---------------------------------------------------------------------------------------
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.n24Override = N24Blocks.Off;
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int fd = FingerprintBaseline.FirstDifference(lines);
            Console.WriteLine($"  fp: composed fingerprint with every NIGHT24 block off: {(fd < 0 ? "EQUAL to the baseline" : "first difference at checkpoint " + (fd + 1))}");
            Need(fd < 0, "the fingerprint differs with the row off");
        }

        if (fails.Count == 0)
        {
            code = 0;
            Console.WriteLine($"wyrm PASS: bot killed the worm at {WyrmMmSs(killStep, dt)} game time (limit 6:00, human target 2:30-3:30), seed {seed}, bot deaths {main.deaths}, {resolves} attacks each told >= 0.5 s ahead and >= {prm.resolveGap}s apart, nearest to spawn {minSpawn:0.0} m, tape EQUAL, fingerprint EQUAL");
        }
        else
        {
            code = 1;
            foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
            Console.WriteLine($"wyrm FAIL: {fails.Count} checks failed (kill {WyrmMmSs(killStep, dt)})");
        }
    }

    static (double lost, int lunge, int body, int head, string note, List<(InputFrame f, bool fire)> tape, ulong hash, int steps) WyrmLungeCase(GameConfig cfg, Dictionary<string, object> boss, WyrmArena arena, string mode)
    {
        var rig = new WyrmRig(cfg, boss, arena, 7, 100);
        Wyrm w = rig.wyrm;
        var tape = new List<(InputFrame, bool)>();
        rig.mover.Reset(0, 0, mode == "slide" ? 5.5 : 0);
        if (mode == "short")
            w.playerBody = () =>
            {
                Vec3 f = rig.mover.position;
                return new Capsule { start = new Vec3(f.x, f.y + 0.35, f.z), end = new Vec3(f.x, f.y + 0.65, f.z), radius = 0.35 }; // the slide's 1 m capsule (config slide.colliderHeight)
            };
        int lungeStart = -1;
        bool crossed = false, slideAtCross = false;
        double zAtCross = 0;
        for (int i = 0; i < 20000; i++)
        {
            var f = new InputFrame();
            if (lungeStart < 0 && w.Lunging) lungeStart = rig.step;
            if (mode == "slide" && lungeStart >= 0 && rig.step - lungeStart < 90)
            {
                if (rig.step - lungeStart >= 20) f.moveZ = 1;
                if (rig.step - lungeStart >= 40) f.slide = new Skygrind.Core.Button { held = true, pressed = rig.step - lungeStart == 40 };
            }
            tape.Add((f, false));
            double hpBefore = rig.health.hp;
            rig.Step(f, false);
            if (rig.health.hp < hpBefore && mode != "slide") Console.WriteLine($"    [{mode}] step {rig.step}: -{hpBefore - rig.health.hp:0} in {w.PatternName}, Lunging {w.Lunging}, head ({w.HeadPosition.x:0.0}, {w.HeadPosition.y:0.0}, {w.HeadPosition.z:0.0}), player ({rig.mover.position.x:0.0}, {rig.mover.position.y:0.0}, {rig.mover.position.z:0.0})");
            if (lungeStart >= 0 && !crossed && Math.Abs(w.HeadPosition.x - rig.mover.position.x) < 1.5)
            {
                crossed = true;
                slideAtCross = rig.mover.state == MoveState.Slide;
                zAtCross = rig.mover.position.z;
            }
            if (lungeStart >= 0 && rig.step - lungeStart > 420) break;
        }
        var st = w.stats;
        string note = mode == "slide" ? $"real slide: state at the head's pass {(slideAtCross ? "SLIDE" : rig.mover.state.ToString())}, z {zAtCross:0.0}" : "";
        return (100 - rig.health.hp, st["contactsLunge"], st["contactsBody"], st["contactsHead"], note, tape, rig.Hash(), rig.step);
    }

    /// <summary>`wyrm --contact`: the paths the bots never took, deterministic: a head contact (damage, push, cooldown), the exposed core (x3),
    /// a body piece's oriented box against a ray an axis-aligned box would get wrong, then the lunge pass against a standing player, a
    /// player under it (the slide's 1 m capsule, and a real scripted slide), and a tape replay of a fight with a contact in it.</summary>
    static void WyrmContact(ref int code)
    {
        GameConfig cfg = Config();
        string repo = Path.GetFullPath(Path.Combine(Root, ".."));
        var boss = Wyrm.LoadBoss(Path.Combine(repo, "Design", "bosses", "wyrm.json"));
        WyrmArena arena = WyrmArena.Load(Path.Combine(repo, "Design", "maps", "wyrm-arena.json"));
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        var idle = new InputFrame();
        WyrmParams prm = WyrmParams.From(cfg, boss);

        // --- a) head contact ---------------------------------------------------------------------------------------------------
        {
            var rig = new WyrmRig(cfg, boss, arena, 7, 100);
            Wyrm w = rig.wyrm;
            for (int i = 0; i < 3000 && !(w.phase == "p1" && !w.buried[0] && w.PatternName == "coil"); i++) rig.Step(idle, false);
            for (int i = 0; i < 60; i++) rig.Step(idle, false);
            WyrmBox h = w.boxes[0];
            Vec3 side = h.c + h.r * 1.2;
            double hp0 = rig.health.hp;
            rig.mover.Reset(side.x, side.y - 0.9, side.z);
            rig.Step(idle, false);
            double lost = hp0 - rig.health.hp;
            Vec3 push = rig.lastPush;
            double ph = JsMath.Hypot(push.x, push.z), dot = ph > 0 ? (push.x * h.r.x + push.z * h.r.z) / ph / JsMath.Hypot(h.r.x, h.r.z) : 0;
            double hp1 = rig.health.hp;
            rig.Step(idle, false);
            bool cooled = rig.health.hp == hp1;
            Console.WriteLine($"  head contact: the player beside the head took {lost:0} (head {prm.contactHead}, lunge {prm.contactLunge}), pushed ({push.x:0.0}, {push.y:0.0}, {push.z:0.0}) = {ph:0.0} m/s away from the head (alignment {dot:0.00}), up {push.y:0.0}; next step no second hit: {cooled}");
            Need(Math.Abs(lost - prm.contactHead) < 1e-9 || Math.Abs(lost - prm.contactLunge) < 1e-9, "head contact damage");
            Need(Math.Abs(ph - prm.pushHead) < 1e-6 && dot > 0.95 && Math.Abs(push.y - prm.pushUp) < 1e-9, "head push");
            Need(cooled, "head contact cooldown");
        }

        // --- b) the exposed core x3 --------------------------------------------------------------------------------------------------
        {
            var rig = new WyrmRig(cfg, boss, arena, 7, 100);
            Wyrm w = rig.wyrm;
            for (int i = 0; i < 3000 && w.phase != "p1"; i++) rig.Step(idle, false);
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            for (int i = 0; i < 120 * 60 && !w.CoreOpen; i++) rig.Step(idle, false);
            WyrmBox h = w.boxes[0];
            Vec3 core = h.c + h.u * prm.coreUp - h.f * prm.coreBack;
            double before = w.body.hp;
            double dmg = w.TakeHit(core + new Vec3(0, 8, 0), new Vec3(0, -1, 0), rig.revolverDamage, "revolver", out double dist);
            double headDmg = 0;
            rig.Step(idle, false);
            Console.WriteLine($"  core hit: core open {w.CoreOpen} (phase {w.phase}); a revolver shot ({rig.revolverDamage:0}) at the core took {dmg:0} off the pool (x{dmg / rig.revolverDamage:0}); body hp {before:0} -> {w.body.hp:0}; core hits counted {w.stats["coreHits"]}");
            Need(Math.Abs(dmg - rig.revolverDamage * prm.coreMult) < 1e-9 && w.stats["coreHits"] == 1, "core x3");
        }

        // --- c) a body piece's oriented box against a ray -----------------------------------------------------------------------------
        {
            var rig = new WyrmRig(cfg, boss, arena, 7, 100);
            Wyrm w = rig.wyrm;
            int pick = -1;
            for (int i = 0; i < 3000 && pick < 0; i++)
            {
                rig.Step(idle, false);
                for (int k = 6; k < 20 && pick < 0; k++)
                    if (!w.buried[k] && Math.Min(Math.Abs(w.boxes[k].f.x), Math.Abs(w.boxes[k].f.z)) > 0.3 && Math.Abs(w.boxes[k].f.y) < 0.4) pick = k;
            }
            WyrmBox b = w.boxes[pick];
            Vec3 inside = b.c + b.r * (b.hr * 0.5) + b.f * (b.hf * 0.5);
            double tIn = b.Ray(inside + b.u * 8, -b.u);
            Vec3 beside = b.c + b.r * (b.hr + 0.3) + b.f * (b.hf * 0.5);
            double tOut = b.Ray(beside + b.u * 8, -b.u);
            // the axis-aligned box of the same piece would take the "beside" ray
            double minx = 1e9, maxx = -1e9, minz = 1e9, maxz = -1e9;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vec3 cr = b.c + b.r * (sx * b.hr) + b.f * (sz * b.hf);
                    minx = Math.Min(minx, cr.x); maxx = Math.Max(maxx, cr.x); minz = Math.Min(minz, cr.z); maxz = Math.Max(maxz, cr.z);
                }
            bool aabbWouldHit = beside.x >= minx && beside.x <= maxx && beside.z >= minz && beside.z <= maxz;
            Console.WriteLine($"  body ray: piece {pick} turned {JsMath.Atan2(b.f.x, b.f.z) * 180 / Math.PI:0} deg off the z axis; a ray down onto its back inside the box hits at {tIn:0.000} m (expected {8 - b.hu:0.000}); a ray 0.3 m beside it: {(tOut < 0 ? "misses" : "HITS")} (an axis-aligned box of the same corners would {(aabbWouldHit ? "take it" : "miss it too")})");
            Need(Math.Abs(tIn - (8 - b.hu)) < 1e-6 && tOut < 0 && aabbWouldHit, "body piece oriented-box ray");
        }

        // --- d) the lunge pass: standing, under it, and a real slide ------------------------------------------------------------------------
        var stand = WyrmLungeCase(cfg, boss, arena, "stand");
        var shortc = WyrmLungeCase(cfg, boss, arena, "short");
        var slide = WyrmLungeCase(cfg, boss, arena, "slide");
        Console.WriteLine($"  lunge, standing on the line: lost {stand.lost:0} hp (lunge head {stand.lunge}, head {stand.head}, body {stand.body} contacts)");
        Console.WriteLine($"  lunge, under it with the slide's 1 m capsule: lost {shortc.lost:0} hp (lunge head {shortc.lunge}, head {shortc.head}, body {shortc.body})");
        Console.WriteLine($"  lunge, a real scripted slide across the line: lost {slide.lost:0} hp (lunge head {slide.lunge}, head {slide.head}, body {slide.body}); {slide.note}");
        Need(stand.lunge + stand.head > 0 && stand.lost >= prm.contactLunge - 1e-9, "a standing player on the lunge line was not hit by the head");
        Need(shortc.lunge == 0 && shortc.head == 0, "a player under the pass was hit by the head");

        // --- e) a tape of a fight with a contact in it replays EQUAL --------------------------------------------------------------------
        {
            var rep = new WyrmRig(cfg, boss, arena, 7, 100);
            rep.mover.Reset(0, 0, 0);
            int diff = -1, k = 0;
            var rec = new WyrmRig(cfg, boss, arena, 7, 100);
            rec.mover.Reset(0, 0, 0);
            var hashes = new List<ulong>();
            for (int i = 0; i < stand.tape.Count; i++)
            {
                rec.Step(stand.tape[i].f, false);
                if (rec.step % 120 == 0) hashes.Add(rec.Hash());
            }
            for (int i = 0; i < stand.tape.Count; i++)
            {
                rep.Step(stand.tape[i].f, false);
                if (rep.step % 120 == 0)
                {
                    if (hashes[k] != rep.Hash()) { diff = rep.step; break; }
                    k++;
                }
            }
            bool eq = diff < 0 && rep.Hash() == rec.Hash();
            Console.WriteLine($"  tape with a contact: {stand.tape.Count} steps (head contacts {rep.wyrm.stats["contactsLunge"] + rep.wyrm.stats["contactsHead"]}, player hp {rep.health.hp:0}, {rep.pushes} pushes): replay {(eq ? "EQUAL" : "DIFFERS at " + diff)} ({k} second-hashes)");
            Need(eq && rep.pushes > 0, "tape with a contact");
        }
        code = fails.Count == 0 ? 0 : 1;
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"wyrm --contact {(fails.Count == 0 ? "PASS" : "FAIL")}");
    }

    /// <summary>`wyrm --composed`: the lag-limited human bot through the COMPOSED SurvivalSim (its player, its HP, the real weapons through the
    /// sim's input, the room as a world layer): the kill time and deaths that matter for the human target (2:30-3:30).</summary>
    static void WyrmComposed(ref int code)
    {
        GameConfig cfg = Config();
        string repo = Path.GetFullPath(Path.Combine(Root, ".."));
        SurvivalSim.Wyrm24Dir = repo;
        double dt = cfg.Dt, sum = 0;
        int killed = 0;
        for (uint sd = 3; sd < 6; sd++)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(sd, null, null);
            sim.husks.frozen = true;
            if (!sim.Wyrm24Begin()) { Console.WriteLine("  composed: Wyrm24Begin failed"); code = 1; return; }
            sim.Respawn(sim.Wyrm24Spawn, 0);
            var rig = new WyrmRig(sim);
            var bot = new WyrmBot(true);
            string guns = string.Join(",", sim.loadout.owned);
            double hpMax = sim.health.max;
            while (sim.wyrm24.Alive && rig.step < 12 * 60 * 120)
            {
                InputFrame f = bot.Next(rig, out bool fire);
                rig.Step(f, fire);
            }
            bool k = !sim.wyrm24.Alive;
            if (k) { killed++; sum += rig.step * dt; }
            var st = sim.wyrm24.stats;
            Console.WriteLine($"  composed seed {sd}: {(k ? "KILLED" : "NOT KILLED")} at {WyrmMmSs(rig.step, dt)}; deaths {rig.deaths}; player max hp {hpMax:0}, guns [{guns}]; shots sent {rig.shotsFired}; worm hits body {st["bodyHits"]} head {st["headHits"]} core {st["coreHits"]}; contacts body {st["contactsBody"]} head {st["contactsHead"]} lunge {st["contactsLunge"]}; spit {st["volleys"]} volleys, {st["shotsHit"]} hit you");
        }
        Console.WriteLine($"wyrm --composed: {killed} of 3 killed, mean {(killed > 0 ? WyrmMmSs((int)(sum / killed / dt), dt) : "n/a")} (human target 2:30-3:30)");
        SurvivalSim.Wyrm24Dir = null;
        code = killed == 3 ? 0 : 1;
    }

    /// <summary>`wyrm --shed`: both bots on 2 seeds with design.n24.wyrm.tailShed on (the row is off by default): the tail cuts loose at each beat,
    /// is shot down or regrows; the attack log keeps its tell rule.</summary>
    static void WyrmShedLine(ref int code)
    {
        GameConfig cfg = Config();
        string repo = Path.GetFullPath(Path.Combine(Root, ".."));
        var boss = Wyrm.LoadBoss(Path.Combine(repo, "Design", "bosses", "wyrm.json"));
        WyrmArena arena = WyrmArena.Load(Path.Combine(repo, "Design", "maps", "wyrm-arena.json"));
        double dt = cfg.Dt;
        int bad = 0;
        foreach (bool human in new[] { false, true })
            for (uint sd = 7; sd < 9; sd++)
            {
                var rig = new WyrmRig(cfg, boss, arena, sd, cfg.Num("health.max"), true);
                var bot = new WyrmBot(human);
                while (rig.wyrm.Alive && rig.step < 12 * 60 * 120)
                {
                    InputFrame f = bot.Next(rig, out bool fire);
                    rig.Step(f, fire);
                }
                var st = rig.wyrm.stats;
                int gap = MovementMath.StepsFor(0.5, dt), tellBad = 0, last = -999999;
                foreach (WyrmLog l in rig.wyrm.log)
                {
                    if (!l.resolve) continue;
                    if (l.step - last < gap) tellBad++;
                    last = l.step;
                }
                bool ok = !rig.wyrm.Alive && st["sheds"] == 2 && tellBad == 0;
                if (!ok) bad++;
                Console.WriteLine($"  shed {(human ? "human" : "bot  ")} seed {sd}: {(rig.wyrm.Alive ? "NOT KILLED" : "killed")} at {WyrmMmSs(rig.step, dt)}; sheds {st["sheds"]}, shot down {st["shedKills"]}, regrown {st["regrows"]}; resolves inside 0.5 s {tellBad}; {(ok ? "ok" : "FAIL")}");
            }
        Console.WriteLine($"wyrm --shed {(bad == 0 ? "PASS" : "FAIL")}");
        code = bad == 0 ? 0 : 1;
    }

    /// <summary>`wyrm --human`: the lag-limited bot (no future, aim 0.25 s late at 4.5 rad/s, shots fired on the reticle not on a hit, the core
    /// after the roar) on 3 seeds. Prints kill time, deaths, hit share and the contacts the omniscient bot never took.</summary>
    static void WyrmHuman(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        string repo = Path.GetFullPath(Path.Combine(Root, ".."));
        var boss = Wyrm.LoadBoss(Path.Combine(repo, "Design", "bosses", "wyrm.json"));
        WyrmArena arena = WyrmArena.Load(Path.Combine(repo, "Design", "maps", "wyrm-arena.json"));
        double dt = cfg.Dt, sum = 0;
        int killed = 0;
        for (uint sd = 7; sd < 10; sd++)
        {
            var rig = new WyrmRig(cfg, boss, arena, sd, cfg.Num("health.max"));
            var bot = new WyrmBot(true);
            while (rig.wyrm.Alive && rig.step < 12 * 60 * 120)
            {
                InputFrame f = bot.Next(rig, out bool fire);
                rig.Step(f, fire);
            }
            bool k = !rig.wyrm.Alive;
            if (k) { killed++; sum += rig.step * dt; }
            var st = rig.wyrm.stats;
            Console.WriteLine($"  human seed {sd}: {(k ? "KILLED" : "NOT KILLED")} at {WyrmMmSs(rig.step, dt)}; deaths {rig.deaths}; shots {rig.shotsHit}/{rig.shotsFired}; body hits {st["bodyHits"]}, head {st["headHits"]}, core {st["coreHits"]}; contacts body {st["contactsBody"]} head {st["contactsHead"]} lunge {st["contactsLunge"]}; spit hits {st["shotsHit"]}");
        }
        Console.WriteLine($"wyrm --human: {killed} of 3 killed, mean {(killed > 0 ? WyrmMmSs((int)(sum / killed / dt), dt) : "n/a")} (human target 2:30-3:30)");
        code = 0;
    }

    /// <summary>Skip a passive fight to a phase: sets the HP below the threshold and lets the beat run.</summary>
    static void ForcePhase(WyrmRig rig, string phase, InputFrame idle)
    {
        Wyrm w = rig.wyrm;
        double frac = phase == "p2" ? 0.6 : 0.3;
        if (phase == "p3" && w.phase == "p1")
        {
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            int guard = 0;
            while (w.phase != "p2" && guard++ < 1000) rig.Step(idle, false);
        }
        w.body.hp = w.body.maxHp * frac;
        w.Apply(0);
    }
}
