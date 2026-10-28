// NIGHT14 U3-27.9 (agent V): WHAT GOT YOU's flight recorder (Game/Recap/FlightRecorder.cs) on a headless composition
// (FingerprintTests.Headless, a live seed-1 run): the ring keeps exactly the last N seconds of steps and wraps; each frame
// holds the player and every live enemy (up to the cap); the killer is the enemy that struck the killing blow (a husk's
// or a swarmer's feet are the blow's origin; a spitter's glob or puddle names its shooter), else the nearest of the
// source's kind, else the nearest enemy; the death's step is the last frame (frozen after it); a new run empties the
// ring; a detached recorder records nothing; and after boot, recording and the event notes allocate nothing (the
// ZeroAllocTests pattern: Unity's GC.Alloc recorder on this thread, warmed first).
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class FlightRecorderTests
    {
        /// <summary>A headless composition with a live run (seed 1; the web's rules and no records, as RunFingerprint runs it).</summary>
        static SurvivalSim Live()
        {
            SurvivalSim sim = FingerprintTests.Headless();
            sim.noRecords = sim.noLab = true; // the headless settings store has no LAB rows
            sim.StartRun(1, null, null);
            return sim;
        }

        /// <summary>One `kind` on the open zone's spawn nodes (the first that fits from the k-th spread pick).</summary>
        static Husk Spawn(SurvivalSim sim, string kind, int k)
        {
            var ground = new List<SpawnNode>();
            var high = new List<SpawnNode>();
            sim.zones.SpawnPoints(ground, high, sim.core);
            Assert.That(ground.Count, Is.GreaterThan(0), "the open zone has spawn nodes");
            for (int i = 0; i < ground.Count; i++)
            {
                SpawnNode n = ground[(k * 7919 + i) % ground.Count];
                Husk h = sim.husks.Spawn(kind, new Vec3(n.x, n.y, n.z), sim.mover.position);
                if (h != null) return h;
            }
            Assert.Fail($"no spawn node fits a {kind}");
            return null;
        }

        [Test]
        public void Capacity_IsTheDesignSecondsOfSteps()
        {
            SurvivalSim sim = Live();
            double seconds = sim.cfg.Num("design.recap.seconds");
            var rec = new FlightRecorder(sim, seconds);
            Assert.That(rec.Capacity, Is.EqualTo((int)System.Math.Round(seconds * sim.cfg.movement.loop.simHz)), "design.recap.seconds of 120 Hz steps (6 s: 720)");
            Assert.That(rec.Count, Is.EqualTo(0));
        }

        [Test]
        public void Ring_KeepsExactlyTheLastSeconds_AndWraps()
        {
            SurvivalSim sim = Live();
            sim.health.max = sim.health.hp = 1e6;
            var rec = new FlightRecorder(sim, 0.05, 8, 8);
            int cap = (int)System.Math.Round(0.05 / sim.step);
            Assert.That(rec.Capacity, Is.EqualTo(cap), "0.05 s at 120 Hz");
            rec.Attach();
            for (int i = 0; i < 20; i++) sim.Step();
            Assert.That(rec.Count, Is.EqualTo(cap), "full after 20 steps: the last cap steps only");
            for (int i = 0; i < cap; i++)
            {
                int step = sim.simStep - cap + 1 + i;
                Assert.That(rec.Frame(i).step, Is.EqualTo(step), $"frame {i} (0 the oldest)");
                Assert.That(rec.Frame(i).time, Is.EqualTo(step * sim.step), $"frame {i}'s sim time");
            }
            Assert.That(rec.NewestTime - rec.OldestTime, Is.EqualTo((cap - 1) * sim.step).Within(1e-9));
            Assert.That(rec.Frame(cap - 1).feet, Is.EqualTo(sim.current), "the newest frame is the step just taken");
            int oldest = rec.Frame(0).step;
            for (int i = 0; i < 3; i++) sim.Step();
            Assert.That(rec.Count, Is.EqualTo(cap), "still full");
            Assert.That(rec.Frame(0).step, Is.EqualTo(oldest + 3), "the window moved by 3 (wrapped)");
            Assert.That(rec.Frame(cap - 1).step, Is.EqualTo(sim.simStep));
            Assert.That(rec.IndexAt(rec.Frame(2).time), Is.EqualTo(2), "IndexAt finds a frame's own time");
            Assert.That(rec.IndexAt(rec.OldestTime - 1), Is.EqualTo(0), "before the window: the oldest");
            Assert.That(rec.IndexAt(rec.NewestTime + 1), Is.EqualTo(cap - 1), "after it: the newest");
            sim.StartRun(2, null, null);
            Assert.That(rec.Count, Is.EqualTo(0), "a new run (RunStarted) empties the ring");
            Assert.That(rec.HasDeath, Is.False);
        }

        [Test]
        public void Frame_HoldsThePlayer_AndEveryLiveEnemy_UpToTheCap()
        {
            SurvivalSim sim = Live();
            sim.health.max = sim.health.hp = 1e6;
            Husk a = Spawn(sim, "husk", 0), b = Spawn(sim, "swarmer", 1), c = Spawn(sim, "spitter", 2);
            var rec = new FlightRecorder(sim, 0.5, 8, 8);
            var capped = new FlightRecorder(sim, 0.5, 2, 8);
            rec.Attach();
            capped.Attach();
            sim.Step();
            ref readonly RecapFrame f = ref rec.Frame(rec.Count - 1);
            Assert.That(f.enemyCount, Is.EqualTo(sim.enemies.AliveCount));
            foreach (Husk h in new[] { a, b, c })
            {
                int k = rec.FindEnemy(f, h.id);
                Assert.That(k, Is.GreaterThanOrEqualTo(0), $"{h.kind} {h.id} is in the frame");
                RecapEnemy r = rec.Enemy(f, k);
                Assert.That(r.position, Is.EqualTo(h.position), $"{h.kind}'s feet after the step");
                Assert.That(r.yaw, Is.EqualTo(h.yaw));
                Assert.That(RecapKind.Name(r.kind), Is.EqualTo(h.kind));
            }
            Assert.That(rec.FindEnemy(f, -12345), Is.EqualTo(-1));
            Assert.That(f.feet, Is.EqualTo(sim.current));
            Assert.That(f.yaw, Is.EqualTo(sim.input.Yaw));
            Assert.That(f.pitch, Is.EqualTo(sim.input.Pitch));
            Assert.That(f.hp, Is.EqualTo(sim.health.hp));
            Assert.That(capped.Frame(capped.Count - 1).enemyCount, Is.EqualTo(2), "a frame keeps at most MaxEnemies");
        }

        [Test]
        public void Killer_HuskBlow_IsTheHuskThatStruck_AndTheDeathStepIsTheLastFrame()
        {
            SurvivalSim sim = Live();
            Husk a = Spawn(sim, "husk", 0), b = Spawn(sim, "husk", 3);
            var rec = new FlightRecorder(sim, 1, 8, 8);
            rec.Attach();
            sim.Step();
            Assert.That(a.position, Is.Not.EqualTo(b.position), "two husks apart");
            sim.health.Damage(sim.health.max * 2, "husk", b.position); // Husk.ResolveAttack: the blow comes from its feet
            Assert.That(sim.flow, Is.EqualTo(Flow.Down));
            Assert.That(rec.HasDeath, Is.True);
            Assert.That(rec.KillerId, Is.EqualTo(b.id));
            Assert.That(rec.KillerHow, Is.EqualTo(RecapKillerHow.Exact));
            Assert.That(rec.KillerKind, Is.EqualTo(RecapKind.Husk));
            Assert.That(rec.KillerSource, Is.EqualTo("husk"));
            Assert.That(rec.KillerName, Is.EqualTo(sim.lastDeath.killer), "the recap's name (HUSK)");
            int n = rec.Count;
            sim.Step();
            Assert.That(rec.Frozen, Is.True, "the first step in DOWN is the last frame");
            Assert.That(rec.Count, Is.EqualTo(n + 1));
            sim.Step();
            sim.Step();
            Assert.That(rec.Count, Is.EqualTo(n + 1), "nothing recorded after the death's step");
        }

        [Test]
        public void Killer_Fallbacks_NearestOfKind_ThenNearest_ThenNone()
        {
            SurvivalSim sim = Live();
            sim.health.max = sim.health.hp = 1e6;
            Husk a = Spawn(sim, "husk", 0), b = Spawn(sim, "husk", 3);
            var rec = new FlightRecorder(sim, 1, 8, 8);
            rec.Attach();
            sim.Step();
            Vec3 near = b.position + new Vec3(0.5, 0, 0);
            Assume.That(Vec3.DistanceTo(a.position, near), Is.GreaterThan(0.5), "b is the husk nearest the blow");
            Assert.That(rec.Resolve("husk", near, true, out RecapKillerHow how), Is.EqualTo(b.id));
            Assert.That(how, Is.EqualTo(RecapKillerHow.NearestOfKind));
            Vec3 me = sim.mover.position;
            Husk nearest = Vec3.DistanceTo(a.position, me) <= Vec3.DistanceTo(b.position, me) ? a : b;
            Assert.That(rec.Resolve("swarmer", near, true, out how), Is.EqualTo(nearest.id), "no swarmer alive: the nearest enemy to you");
            Assert.That(how, Is.EqualTo(RecapKillerHow.Nearest));
            Assert.That(rec.Resolve("fall", me, false, out how), Is.EqualTo(-1), "THE FALL has no enemy");
            Assert.That(how, Is.EqualTo(RecapKillerHow.None));
        }

        [Test]
        public void Killer_SpitterGlobOrPuddle_IsItsShooter()
        {
            SurvivalSim sim = Live();
            sim.health.max = sim.health.hp = 1e6;
            Husk s = Spawn(sim, "spitter", 0);
            var rec = new FlightRecorder(sim, 1, 8, 8);
            rec.Attach();
            sim.Step();
            SpitterAcid.Glob g = sim.husks.acid.globs[0];
            g.alive = true;
            g.reflected = false;
            g.shooter = s;
            g.position = sim.mover.position + new Vec3(0.3, 1, 0);
            g.velocity = new Vec3(-6, -1, 2);
            Assert.That(rec.Resolve("spitter", FlightRecorder.GlobFrom(g), true, out RecapKillerHow how), Is.EqualTo(s.id), "the glob's from (Spitter.Run's arc) names its shooter");
            Assert.That(how, Is.EqualTo(RecapKillerHow.Shooter));
            g.alive = false;
            SpitterAcid.Puddle pd = sim.husks.acid.puddles[0];
            pd.alive = true;
            pd.position = sim.mover.position;
            pd.shooter = s;
            Assert.That(rec.Resolve("acid", default, false, out how), Is.EqualTo(s.id), "the puddle you stand in names its shooter");
            Assert.That(how, Is.EqualTo(RecapKillerHow.Shooter));
            pd.alive = false;
        }

        [Test]
        public void Detach_StopsRecording_AndTheEvents()
        {
            SurvivalSim sim = Live();
            var rec = new FlightRecorder(sim, 1, 8, 8);
            rec.Attach();
            Assert.That(rec.Attached, Is.True);
            for (int i = 0; i < 3; i++) sim.Step();
            Assert.That(rec.Count, Is.EqualTo(3));
            rec.Detach();
            Assert.That(rec.Attached, Is.False);
            for (int i = 0; i < 3; i++) sim.Step();
            Assert.That(rec.Count, Is.EqualTo(3), "no StepEnded subscription");
            sim.health.Damage(sim.health.max * 2, "fall");
            Assert.That(rec.HasDeath, Is.False, "no death subscription");
        }

        /// <summary>GC allocation samples made by `a` on this thread (ZeroAllocTests.Allocs' recorder logic).</summary>
        static int Allocs(System.Action a)
        {
            var recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;
            try
            {
                a();
            }
            finally
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }
            return recorder.sampleBlockCount;
        }

        [Test]
        public void RecordAndNotes_AfterBoot_AllocateNothing()
        {
            SurvivalSim sim = Live();
            sim.health.max = sim.health.hp = 1e6;
            Husk first = null;
            for (int k = 0; k < 6; k++)
            {
                Husk h = Spawn(sim, "husk", k);
                first ??= h;
            }
            for (int k = 6; k < 9; k++) Spawn(sim, "swarmer", k);
            for (int k = 9; k < 11; k++) Spawn(sim, "spitter", k);
            var rec = new FlightRecorder(sim, sim.cfg.Num("design.recap.seconds"));
            rec.Attach();
            for (int i = 0; i < 30; i++) sim.Step(); // the frames, the handlers and the JIT warm
            SpitterAcid.Glob g = sim.husks.acid.globs[0];
            g.alive = true;
            g.shooter = (Husk)sim.husks.PoolOf("spitter")[0];
            g.position = sim.mover.position + new Vec3(4, 3, 0);
            g.velocity = new Vec3(-1, 0, 0);
            var hit = new HitEvent { owner = "player", enemy = first.id, kind = first.kind, position = first.position };
            var kill = new KillEvent { owner = "player", enemy = first.id, kind = first.kind, position = first.position };
            var hurt = new HurtEvent { owner = "player", amount = 1, hp = 99, source = "husk", from = first.position, hasFrom = true };
            void Burst()
            {
                for (int i = 0; i < 600; i++)
                {
                    rec.NoteHit(hit);
                    rec.NoteKill(kill);
                    rec.NoteHurt(hurt);
                    rec.Record();
                }
            }
            Burst(); // warm the JIT on every path
            int allocs = Allocs(Burst);
            Assert.That(allocs, Is.EqualTo(0), "GC allocation samples over 600 Record + 1800 notes");
            Assert.That(rec.Count, Is.EqualTo(rec.Capacity), "the ring wrapped (30 + 1200 frames)");
            g.alive = false;
        }
    }
}
