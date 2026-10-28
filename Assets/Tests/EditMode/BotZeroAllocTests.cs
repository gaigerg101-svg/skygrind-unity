// NIGHT14 U3-27.4 (agent N): the bot's per-step paths allocate nothing (PORT-RULES 6). On the Spire (the duel graph,
// core 'towers'), a bot chases a target running a loop: BotBrain.Step plans (A*), follows (Follower.Step), engages
// (the rng's strafe and aim rerolls), evades a shot (onPlayerFire) and recovers at low HP (hideSpot); a follower alone
// walks a long planned path. After a warm-up (the JIT, the A* scratch, the path lists grown to size), every step's GC
// allocation samples are counted on their own (the mover's step is outside the count): 0.
// The pattern of ZeroAllocTests: the delegates are made once, before the count, and warmed.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class BotZeroAllocTests
    {
        /// <summary>GC allocation samples made by `a` on this thread (ZeroAllocTests' recorder logic).</summary>
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

        sealed class BotRig
        {
            public readonly Mover body;
            public readonly Health health;
            public readonly BotTarget target = new BotTarget();
            public readonly BotBrain brain;
            public BotFrame frame;
            public int step;
            readonly double dt;
            readonly Vec3 spawn;
            public readonly System.Action brainStep;

            public BotRig()
            {
                MovementConfig m = BotSpire.Config.movement;
                dt = 1 / m.loop.simHz;
                body = BotSpire.Body(BotSpire.DuelSpawn(0));
                health = new Health(100, new EventBus(), "rival");
                var deps = new BotDeps { getPlayer = Target, ray = BotSpire.World.RayIntersect, getGraph = Graph, rng = new Mulberry32(3) };
                deps.orbs.Add(new BotOrb { position = new Vec3(0, 0, 0), active = false });
                brain = new BotBrain(BotSpire.Config, new BotSelf { mover = body, health = health, weapon = Weapon }, deps);
                brain.Reset("hard");
                spawn = BotSpire.DuelSpawn(1);
                brainStep = BrainStep;
            }

            BotTarget Target() => target;

            static NavGraph Graph() => BotSpire.Graph;

            static string Weapon() => "revolver";

            void BrainStep() => brain.Step(ref frame, dt);

            /// <summary>Move the target, step the brain (counted or not), then the body.</summary>
            public int Step(bool count)
            {
                double a = step * dt * 0.4;
                target.position = new Vec3(spawn.x + 8 * JsMath.Sin(a), 0, spawn.z + 8 * JsMath.Cos(a));
                target.velocity = new Vec3(3.2 * JsMath.Cos(a), 0, -3.2 * JsMath.Sin(a));
                if (step % 900 == 450) health.hp = 20; // RECOVER (hideSpot)
                if (step % 900 == 0) health.hp = 100;
                if (step % 300 == 150 && brain.visible) brain.OnPlayerFire(new Vec3(target.position.x, 1.6, target.position.z), Vec3.Normalize(body.position - target.position)); // EVADE
                frame = new BotFrame();
                int n = 0;
                if (count) n = Allocs(brainStep);
                else brainStep();
                body.Step(frame.move, dt);
                step++;
                return n;
            }
        }

        [Test]
        public void BotBrainStep_ChaseEngageEvadeRecover_AllocatesNothing()
        {
            var rig = new BotRig();
            var states = new HashSet<string>();
            for (int i = 0; i < 1800; i++)
            {
                rig.Step(false);
                states.Add(rig.brain.State);
            }
            int allocs = 0;
            for (int i = 0; i < 1800; i++)
            {
                allocs += rig.Step(true);
                states.Add(rig.brain.State);
            }
            TestContext.WriteLine($"states: {string.Join(", ", states)}; plans {rig.brain.navPlans}; GC allocation samples over 1800 BotBrain.Step: {allocs}");
            Assert.IsTrue(states.Contains("chase") && states.Contains("recover"), "the run chased and recovered");
            Assert.AreEqual(0, allocs, "GC allocation samples over 1800 BotBrain.Step");
        }

        [Test]
        public void FollowerStep_AlongAPlannedPath_AllocatesNothing()
        {
            NavGraph graph = BotSpire.Graph;
            MovementConfig m = BotSpire.Config.movement;
            double dt = 1 / m.loop.simHz;
            var follower = new Follower(BotSpire.Config);
            var path = new NavPath();
            Mover body = BotSpire.Body(BotSpire.DuelSpawn(0));
            var frame = new InputFrame();
            FollowStatus status = FollowStatus.Idle;
            System.Action step = () => status = follower.Step(body, ref frame, dt);
            int goal = Astar.NearestNode(graph, BotSpire.DuelSpawn(1));
            int home = Astar.NearestNode(graph, BotSpire.DuelSpawn(0));
            int allocs = 0, legs = 0;
            for (int i = 0; i < 4800; i++)
            {
                if (!follower.Active)
                {
                    int from = Astar.NearestNode(graph, body.position);
                    Assert.IsTrue(Astar.Path(graph, from, legs % 2 == 0 ? goal : home, m.dash.speed, path));
                    follower.SetPath(graph, path);
                    legs++;
                }
                frame = new InputFrame();
                if (i < 1200) step();
                else allocs += Allocs(step);
                body.Step(frame, dt);
            }
            TestContext.WriteLine($"{legs} legs planned; GC allocation samples over 3600 Follower.Step: {allocs}");
            Assert.AreEqual(0, allocs, "GC allocation samples over 3600 Follower.Step");
        }
    }
}
