// NIGHT24 leftover 8 (builder B): `monolith [--seed N] [--cap M]` MONOLITH's headless fight (Core/Enemies/Monolith.cs), standalone: the boss in its own
// flat arena (Design/maps/monolith-arena.json) with a bot player (the real Mover on the arena's boxes with the boss's body as a world layer, the real
// Health, the revolver's damage and interval from config) and the real BossKit. One hook line in the lead's Harness.N24.cs dispatch:
//     else if (cmd == "monolith") MonolithCommand(args, ref code);
// Prints one line per check and the contract line; exit 1 on a FAIL: the kill time, the attack log (every tell >= 0.5 s), a tape replay EQUAL, the web's
// boss events, the fingerprint EQUAL with every NIGHT24 block off.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class MonoRig
    {
        public readonly GameConfig cfg;
        public readonly MonoWorld world;
        public readonly EventBus events = new EventBus();
        public readonly Health health;
        public readonly Mover mover;
        public readonly BossKit kit;
        public readonly Monolith boss;
        public readonly WyrmArena arena;
        public readonly double dt, revolverDamage;
        public readonly int revolverInterval;
        public int step, fireCool, deaths, shots, shotsHit;
        public readonly List<(int step, string phase)> phases = new List<(int, string)>();

        public MonoRig(GameConfig cfg, WyrmArena arena, string arenaFile, uint seed)
        {
            this.cfg = cfg;
            this.arena = arena;
            dt = cfg.Dt;
            world = new MonoWorld(arena.BuildWorld(Vec3.Zero));
            health = new Health(cfg.Num("health.max"), events, "player");
            mover = new Mover(cfg.movement, world);
            mover.Reset(arena.spawn.x, arena.spawn.y, arena.spawn.z);
            var target = new BossTarget { position = () => mover.position, health = health, impulse = (x, y, z) => mover.Impulse(x, y, z) };
            kit = new BossKit(cfg, events, null, target);
            int ids = 0;
            boss = new Monolith(cfg, events, world, target, () => ++ids);
            boss.playerBody = () => mover.Body;
            boss.playerVelocity = () => mover.velocity;
            world.layer = boss.layer;
            kit.bosses["monolith"] = boss;
            boss.NewRun(seed ^ (uint)boss.b.salt);
            revolverDamage = cfg.Num("weapons.revolver.damage");
            revolverInterval = MovementMath.StepsFor(cfg.Num("weapons.revolver.fireInterval"), dt);
            events.On<BossEvent>("bossPhase", e => phases.Add((step, e.phase)));
            kit.Start("monolith");
        }

        public Vec3 Eye => new Vec3(mover.position.x, mover.position.y + 1.6, mover.position.z);

        public void Step(in InputFrame f, bool fire)
        {
            mover.Step(f, dt);
            if (fireCool > 0) fireCool--;
            if (fire && fireCool == 0)
            {
                fireCool = revolverInterval;
                shots++;
                Vec3 d = WyrmRig.AimDir(f.yaw, f.pitch);
                double t = boss.Raycast(Eye, d);
                bool blockedByWall = world.RayIntersect(Eye, d, out RayHit hit) && t >= 0 && hit.distance < t;
                if (t >= 0 && !blockedByWall && boss.TakeHit(Eye, d, revolverDamage, out _) > 0) shotsHit++;
            }
            boss.Step(dt);
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
            Mix(boss.body.hp);
            Mix(boss.body.position.x);
            Mix(boss.body.position.y);
            Mix(boss.body.position.z);
            Mix(health.hp);
            Mix(mover.position.x);
            Mix(mover.position.y);
            Mix(mover.position.z);
            Mix(boss.RngState);
            Mix(boss.orbs.Count);
            Mix(boss.RingCount);
            foreach (char ch in boss.phase) Mix(ch);
            return h;
        }
    }

    /// <summary>The player bot: keeps ~16 m from the boss, shoots its core at a limited turn rate, jumps the rings and the laser, steps out of the surge circles and
    /// off the sweep's line.</summary>
    sealed class MonoBot
    {
        double yaw, pitch;
        const double AimRate = 6.0;

        static double Wrap(double a)
        {
            while (a > Math.PI) a -= 2 * Math.PI;
            while (a < -Math.PI) a += 2 * Math.PI;
            return a;
        }

        public InputFrame Next(MonoRig r, out bool fire)
        {
            Monolith m = r.boss;
            Vec3 me = r.mover.position;
            Vec3 bp = m.body.position;
            // aim at the core
            Vec3 core = m.CoreWorld();
            Vec3 d = core - r.Eye;
            double wantYaw = JsMath.Atan2(-d.x, -d.z), wantPitch = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            double st = AimRate * r.dt;
            yaw += Math.Max(-st, Math.Min(st, Wrap(wantYaw - yaw)));
            pitch += Math.Max(-st, Math.Min(st, wantPitch - pitch));
            fire = m.Alive && m.Raycast(r.Eye, WyrmRig.AimDir(yaw, pitch)) >= 0 && m.hitPart == "core" && r.fireCool == 0;
            // where to be
            double dx = me.x - bp.x, dz = me.z - bp.z, dist = JsMath.Hypot(dx, dz);
            if (dist < 1e-6) { dx = 0; dz = 1; dist = 1; }
            double rx = dx / dist, rz = dz / dist; // radial, out
            double tx = -rz, tz = rx; // tangent
            double mx = 0, mz = 0;
            double want = 17;
            mx += rx * Math.Max(-1, Math.Min(1, (want - dist) / 4));
            mz += rz * Math.Max(-1, Math.Min(1, (want - dist) / 4));
            mx += tx * 0.5;
            mz += tz * 0.5;
            // a surge circle under or near me: out of it
            for (int i = 0; i < m.WarnCount; i++)
            {
                m.Warn(i, out double wx, out double wy, out double wz, out double wr, out double share);
                if (wr > 5) continue; // the slam's wide circle is the boss's own
                double ex = me.x - wx, ez = me.z - wz, ed = JsMath.Hypot(ex, ez);
                if (ed < wr + 2.5)
                {
                    if (ed < 1e-6) { ex = 1; ez = 0; ed = 1; }
                    mx += ex / ed * 3;
                    mz += ez / ed * 3;
                }
            }
            // the sweep's painted line: off it
            if (m.SweepLive)
            {
                Vec3 t = me;
                double sx = me.x - bp.x, sz = me.z - bp.z;
                double along = (sx * (me.x - bp.x) + sz * (me.z - bp.z));
                mx += tx * 2;
                mz += tz * 2;
            }
            bool jump = false;
            for (int i = 0; i < m.RingCount; i++)
                if (m.RingLive(i) && Math.Abs(dist - m.RingRadius(i)) < 3.2) jump = true;
            if (m.LaserLive && !m.LaserCharging)
            {
                double pa = JsMath.Atan2(me.z - bp.z, me.x - bp.x);
                if (Math.Abs(Wrap(pa - m.LaserAngle)) < 0.5) jump = true;
            }
            var f = new InputFrame { yaw = yaw, pitch = pitch };
            double ml = JsMath.Hypot(mx, mz);
            if (ml > 0.05)
            {
                mx /= Math.Max(1, ml);
                mz /= Math.Max(1, ml);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yaw), right = MovementMath.WishDirection(1, 0, yaw);
                f.moveZ = mx * fwd.x + mz * fwd.z;
                f.moveX = mx * right.x + mz * right.z;
            }
            if (jump) f.jump = new Skygrind.Core.Button { held = true, pressed = true };
            return f;
        }
    }

    /// <summary>The arena's triangles plus the boss's body layer (solid to the player, rays: false as boss.js).</summary>
    sealed class MonoWorld : ICollisionWorld
    {
        readonly TriangleWorld tw;
        public BoxLayer layer;
        readonly List<ThreeOctree.Triangle> extra = new List<ThreeOctree.Triangle>();
        int baseCount;

        public MonoWorld(TriangleWorld tw) { this.tw = tw; }

        public int GatherCandidates(in Capsule capsule)
        {
            baseCount = tw.GatherCandidates(capsule);
            extra.Clear();
            layer?.GetCapsuleTriangles(capsule, extra);
            return baseCount + extra.Count;
        }

        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact)
        {
            if (candidate < baseCount) return tw.CapsuleIntersect(capsule, candidate, out contact);
            ThreeOctree.Triangle t = extra[candidate - baseCount];
            return TriangleWorld.TriangleCapsuleIntersect(capsule, t.a, t.b, t.c, out contact);
        }

        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit) => tw.RayIntersect(origin, direction, out hit);
    }

    public static void MonolithCommand(string[] args, ref int code)
    {
        uint seed = 7;
        double capMin = 12;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seed") seed = uint.Parse(args[i + 1]);
            if (args[i] == "--cap") capMin = double.Parse(args[i + 1]);
        }
        GameConfig cfg = Config();
        string repo = Path.GetFullPath(Path.Combine(Root, ".."));
        string arenaFile = Path.Combine(repo, "Design", "maps", "monolith-arena.json");
        WyrmArena arena = WyrmArena.Load(arenaFile);
        double dt = cfg.Dt;
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Console.WriteLine($"monolith: row design.n24.monolith.enabled = {cfg.Design("n24.monolith.enabled")}; the fight is explicit; seed {seed}");

        var rig = new MonoRig(cfg, arena, arenaFile, seed);
        var bot = new MonoBot();
        var tape = new List<(InputFrame f, bool fire)>();
        var hashes = new List<ulong>();
        int cap = (int)(capMin * 60 / dt);
        while (rig.boss.Alive && rig.step < cap)
        {
            InputFrame f = bot.Next(rig, out bool fire);
            tape.Add((f, fire));
            rig.Step(f, fire);
            if (rig.step % 120 == 0) hashes.Add(rig.Hash());
        }
        bool killed = !rig.boss.Alive;
        Console.WriteLine($"  fight: {(killed ? "KILLED" : "NOT KILLED")} at {WyrmMmSs(rig.step, dt)} game time ({rig.step} steps); bot deaths {rig.deaths}, player hp {rig.health.hp:0}; shots {rig.shotsHit}/{rig.shots} landed; boss max hp {rig.boss.body.maxHp:0}");
        Console.WriteLine($"  phases: {string.Join(", ", rig.phases.ConvertAll(x => $"{x.phase}@{WyrmMmSs(x.step, dt)}"))}");
        var st = rig.boss.stats;
        Console.WriteLine($"  stats: core hits {st["coreHits"]}, body hits {st["bodyHits"]}; slams {st["slams"]} (ring hits on the bot {st["ringHits"]}), volleys {st["volleys"]} (orb hits {st["orbHits"]}), kneels {st["kneels"]}, surges {st["surges"]} (hits {st["surgeHits"]}), sweeps {st["sweeps"]} (hits {st["sweepHits"]}), lasers {st["lasers"]} (hits {st["laserHits"]})");
        Need(killed, "the bot did not kill MONOLITH");
        Need(killed && rig.step * dt <= 360, $"the kill took {WyrmMmSs(rig.step, dt)}, over 6 minutes");
        Need(st["slams"] > 0 && st["volleys"] > 0 && st["kneels"] > 0 && st["surges"] > 0 && st["sweeps"] > 0 && st["lasers"] > 0, "an attack of the core set never came up");

        int minTell = MovementMath.StepsFor(0.5, dt), bad = 0;
        foreach (var t in rig.boss.tells)
            if (MovementMath.StepsFor(t.seconds, dt) < minTell) bad++;
        Console.WriteLine($"  tells: {rig.boss.tells.Count} shown, under 0.5 s: {bad}");
        Need(bad == 0, "a tell under 0.5 s");
        string[] want = { "bossIntro", "bossBeat", "bossTell", "bossPhase", "bossHit", "bossDying", "bossDown", "bossRise", "bossRoar", "bossSlam", "bossVolley", "bossKneel" };
        var missing = new List<string>();
        foreach (string e in want)
            if (!rig.events.Emitted.ContainsKey(e)) missing.Add(e);
        Console.WriteLine($"  events: {(missing.Count == 0 ? "the web's boss events emitted (bossIntro, bossBeat, bossTell, bossPhase, bossHit, bossRise, bossRoar, bossSlam, bossVolley, bossKneel, bossDying, bossDown)" : "MISSING " + string.Join(", ", missing))}");
        Need(missing.Count == 0, "boss events missing: " + string.Join(", ", missing));

        var rep = new MonoRig(cfg, arena, arenaFile, seed);
        int diff = -1, rh = 0;
        for (int i = 0; i < tape.Count && rep.boss.Alive; i++)
        {
            rep.Step(tape[i].f, tape[i].fire);
            if (rep.step % 120 == 0)
            {
                if (rh >= hashes.Count || hashes[rh] != rep.Hash()) { diff = rep.step; break; }
                rh++;
            }
        }
        bool equal = diff < 0 && rep.step == rig.step && rep.boss.body.hp == rig.boss.body.hp;
        Console.WriteLine($"  tape: {tape.Count} steps replayed: {(equal ? "EQUAL" : "DIFFERS at step " + diff)} ({rh} second-hashes compared)");
        Need(equal, "the tape replay differs");

        {
            SurvivalSim.Wyrm24Dir = repo;
            SurvivalSim cs = SurvivalSim.Headless(cfg, Map());
            cs.noLab = cs.noRecords = true;
            cs.monolith24Force = true;
            cs.setup.mode = RunSetup.Survival;
            cs.StartRun(3, null, null);
            bool began = cs.Monolith24Begin();
            Need(began, "Monolith24Begin did not start the fight");
            if (began)
            {
                cs.Respawn(cs.Monolith24Spawn, 0);
                cs.husks.frozen = true;
                for (int i = 0; i < 1500; i++) cs.Step();
                Monolith cm = cs.monolith24;
                bool reg = cs.enemies.all.Contains(cm.body);
                Vec3 eye = cs.mover.position + new Vec3(0, 1.6, 0);
                double rc = cm.Raycast(eye, Vec3.Normalize(cm.CoreWorld() - eye)); // from the player to the core (the core sits inside the body's height: from above the body is hit first)
                double dealt = EnemyHits.Filter(cm.body, 25, false, "revolver", false, cm.CoreWorld(), cs.events, out bool blocked);
                Console.WriteLine($"  sim: began {began}, registered {reg}, after 1500 steps phase {cm.phase}, kit alive {cs.bossKit.Alive}, a shot at the core through EnemyHits.Filter lands {dealt} (x{(blocked ? 0 : dealt / 25):0.0}), blocked {blocked}");
                Need(reg && cm.Alive && cs.bossKit.Alive && !blocked && Math.Abs(dealt - 25 * cm.b.coreMult) < 1e-9, "MONOLITH is not alive and hittable inside the sim");
            }
            SurvivalSim.Wyrm24Dir = null;
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
            Console.WriteLine($"monolith PASS: bot killed MONOLITH at {WyrmMmSs(rig.step, dt)} game time (limit 6:00), seed {seed}, bot deaths {rig.deaths}, {rig.boss.tells.Count} tells each >= 0.5 s, tape EQUAL, fingerprint EQUAL");
        }
        else
        {
            code = 1;
            foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
            Console.WriteLine($"monolith FAIL: {fails.Count} checks failed (kill {WyrmMmSs(rig.step, dt)})");
        }
    }
}
